using AbuAmenPharma.Data;
using AbuAmenPharma.Models;
using AbuAmenPharma.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Admin,Operator")]
public class PurchasesController : Controller
{
    private readonly ApplicationDbContext _context;
    public PurchasesController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index()
    {
        var data = await _context.Purchases
            .AsNoTracking()
            .Include(x => x.Supplier)
            .OrderByDescending(x => x.Id)
            .Take(200)
            .ToListAsync();

        return View(data);
    }

    [HttpPost]
    public async Task<IActionResult> GetPurchaseData()
    {
        try
        {
            // قراءة بيانات DataTable من الـ Request
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault()?.ToLower() ?? "";

            int pageSize = length != null ? int.Parse(length) : 25;
            int skip = start != null ? int.Parse(start) : 0;

            // الاستعلام الأساسي
            var query = _context.Purchases
                .AsNoTracking()
                .Include(x => x.Supplier)
                .AsQueryable();

            // العدد الإجمالي
            int totalRecords = await query.CountAsync();

            // تطبيق البحث
            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(x =>
                    x.Id.ToString().Contains(searchValue) ||
                    (x.Supplier != null && x.Supplier.Name.Contains(searchValue)) ||
                    x.NetTotal.ToString().Contains(searchValue)
                );
            }

            int filteredRecords = await query.CountAsync();

            // جلب البيانات مع التقسيم
            var data = await query
                .OrderByDescending(x => x.Id)
                .Skip(skip)
                .Take(pageSize)
                .Select(x => new PurchaseRow
                {
                    id = (int)x.Id,
                    purchaseDate = x.PurchaseDate.ToString("yyyy-MM-dd"),
                    supplierName = x.Supplier != null ? x.Supplier.Name : "-",
                    netTotal = x.NetTotal,
                    isPosted = x.IsPosted
                })
                .ToListAsync();

            // تجهيز الاستجابة
            var response = new PurchaseDataTableResponse
            {
                draw = int.TryParse(draw, out var drawValue) ? drawValue : 1,
                recordsTotal = totalRecords,
                recordsFiltered = filteredRecords,
                data = data
            };

            return Ok(response);
        }
        catch (Exception)
        {
            var fallbackDraw = int.TryParse(Request.Form["draw"].FirstOrDefault(), out var parsedDraw)
                ? parsedDraw
                : 1;

            return Ok(new PurchaseDataTableResponse
            {
                draw = fallbackDraw,
                recordsTotal = 0,
                recordsFiltered = 0,
                data = new List<PurchaseRow>()
            });
        }
    }

    public async Task<IActionResult> Details(long id)
    {
        var purchase = await _context.Purchases
            .AsNoTracking()
            .Include(p => p.Supplier)
            .Include(p => p.Lines)
                .ThenInclude(l => l.Item)
            .Include(p => p.Lines)
                .ThenInclude(l => l.Batch)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (purchase == null) return NotFound();

        return View(purchase);
    }

    // (اختياري) للطباعة
    public async Task<IActionResult> Print(long id)
    {
        var purchase = await _context.Purchases
            .AsNoTracking()
            .Include(p => p.Supplier)
            .Include(p => p.Lines).ThenInclude(l => l.Item)
            .Include(p => p.Lines).ThenInclude(l => l.Batch)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (purchase == null) return NotFound();

        return View(purchase);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.SupplierId = new SelectList(
            await _context.Suppliers.Where(x => x.IsActive).OrderBy(x => x.Name).ToListAsync(),
            "Id", "Name"
        );

        return View(new PurchaseCreateVM());
    }

    // ✅ API لـ Select2 (بحث أصناف)
    [HttpGet]
    public async Task<IActionResult> SearchItems(string term)
    {
        term = (term ?? "").Trim();

        var q = _context.Items.AsNoTracking()
            .Where(x => x.IsActive);

        if (!string.IsNullOrWhiteSpace(term))
            q = q.Where(x => x.NameAr.Contains(term) || (x.BarCode != null && x.BarCode.Contains(term)));

        var items = await q.OrderBy(x => x.NameAr)
            .Take(20)
            .Select(x => new { id = x.Id, text = x.NameAr + (x.BarCode != null ? $" - {x.BarCode}" : "") })
            .ToListAsync();

        return Json(items);
    }

    [HttpGet]
    public async Task<IActionResult> GetItemPrice(int id)
    {
        var price = await _context.Items.Where(x => x.Id == id).Select(x => x.DefaultPurchasePrice).FirstOrDefaultAsync();
        return Json(new { price });
    }

    [HttpPost]
    public async Task<IActionResult> CreateSupplierAjax(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest("اسم المورد مطلوب");

        var supplier = new Supplier { Name = name.Trim(), IsActive = true };
        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        return Json(new { id = supplier.Id, text = supplier.Name }); // Return text for Select2 compatibility
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PurchaseCreateVM vm)
    {
        if (vm.Lines == null || vm.Lines.Count == 0)
            ModelState.AddModelError("", "أضف بند واحد على الأقل.");

        if (!ModelState.IsValid)
        {
            ViewBag.SupplierId = new SelectList(
                await _context.Suppliers.Where(x => x.IsActive).OrderBy(x => x.Name).ToListAsync(),
                "Id", "Name", vm.SupplierId
            );
            return View(vm);
        }

        using var trx = await _context.Database.BeginTransactionAsync();

        // 1) إنشاء Purchase
        var purchase = new Purchase
        {
            PurchaseDate = vm.PurchaseDate,
            SupplierId = vm.SupplierId,
            InvoiceNo = vm.InvoiceNo?.Trim() ?? "",
            Discount = vm.Discount,
            Notes = vm.Notes?.Trim()
        };

        // 2) إنشاء Lines + Batch كـ Entity وربطه بالـ Line (بدون BatchId)
        var lines = vm.Lines ?? new List<PurchaseLineVM>();

        foreach (var line in lines)
        {
            if (line.ItemId <= 0 || line.Qty <= 0 || line.UnitCost < 0)
                continue;

            var batch = await BuildNextBatchAsync(line.ItemId, line.ExpiryDate, line.UnitCost);

            var pl = new PurchaseLine
            {
                ItemId = line.ItemId,
                ExpiryDate = line.ExpiryDate,
                Qty = line.Qty,
                UnitCost = line.UnitCost,
                LineTotal = line.Qty * line.UnitCost,

                Batch = batch // ✅ هنا السر: EF سيحفظ batch ثم يملأ BatchId تلقائياً
            };

            purchase.Lines.Add(pl);
        }

        if (purchase.Lines.Count == 0)
        {
            ModelState.AddModelError("", "بنود الفاتورة غير صحيحة.");
            await trx.RollbackAsync();

            ViewBag.SupplierId = new SelectList(
                await _context.Suppliers.Where(x => x.IsActive).OrderBy(x => x.Name).ToListAsync(),
                "Id", "Name", vm.SupplierId
            );
            return View(vm);
        }

        purchase.SubTotal = purchase.Lines.Sum(x => x.LineTotal);
        purchase.NetTotal = purchase.SubTotal - purchase.Discount;

        // 3) حفظ الفاتورة (سيحفظ معها Lines وBatches)
        _context.Purchases.Add(purchase);
        await _context.SaveChangesAsync();

        // 4) الآن BatchId صار موجود داخل كل Line ✅
        foreach (var l in purchase.Lines)
        {
            _context.StockMovements.Add(new StockMovement
            {
                Date = purchase.PurchaseDate,
                ItemId = l.ItemId,
                BatchId = l.BatchId,     // ✅ جاهز
                QtyIn = l.Qty,
                QtyOut = 0,
                UnitCost = l.UnitCost,
                RefType = StockRefType.Purchase,
                RefId = purchase.Id,
                Notes = $"فاتورة شراء رقم: {purchase.Id}"
            });
        }

        purchase.IsPosted = true;
        await _context.SaveChangesAsync();

        await trx.CommitAsync();
        TempData["SuccessMessage"] = $"تم حفظ الفاتورة بنجاح - المورد: {(await _context.Suppliers.FindAsync(vm.SupplierId))?.Name} | إجمالي: {purchase.NetTotal:N2}";
        return RedirectToAction(nameof(Index));
    }

    // ✅ إنشاء BatchNo تلقائي (1,2,3...) لكل صنف
    private async Task<ItemBatch> BuildNextBatchAsync(int itemId, DateOnly expiry, decimal purchasePrice)
    {
        // آخر رقم دفعة لنفس الصنف
        var lastBatchNoStr = await _context.ItemBatches
            .Where(x => x.ItemId == itemId)
            .OrderByDescending(x => x.Id)
            .Select(x => x.BatchNo)
            .FirstOrDefaultAsync();

        int next = 1;
        if (!string.IsNullOrEmpty(lastBatchNoStr) && int.TryParse(lastBatchNoStr, out var last))
            next = last + 1;

        var sellPrice = await _context.Items
            .Where(x => x.Id == itemId)
            .Select(x => x.DefaultSellPrice)
            .FirstAsync();

        return new ItemBatch
        {
            ItemId = itemId,
            BatchNo = next.ToString(),
            ExpiryDate = expiry,
            PurchasePrice = purchasePrice,
            SellPrice = sellPrice,
            IsActive = true
        };
    }
}

