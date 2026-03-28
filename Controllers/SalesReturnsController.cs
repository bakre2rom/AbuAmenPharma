using AbuAmenPharma.Data;
using AbuAmenPharma.Models;
using AbuAmenPharma.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Admin,Operator")]
public class SalesReturnsController : Controller
{
    private readonly ApplicationDbContext _context;
    public SalesReturnsController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index()
    {
        var data = await _context.SaleReturns
            .AsNoTracking()
            .Where(x => x.IsActive)
            .Include(x => x.Customer)
            .Include(x => x.Sale)
            .OrderByDescending(x => x.Id)
            .Take(300)
            .ToListAsync();

        return View(data);
    }

    public async Task<IActionResult> Create()
    {
        var sales = await _context.Sales
            .AsNoTracking()
            .Include(x => x.Customer)
            .Where(x => x.IsPosted)
            .OrderByDescending(x => x.Id)
            .Take(200)
            .ToListAsync();

        return View(sales);
    }

    public async Task<IActionResult> CreateFromSale(long saleId)
    {
        var sale = await _context.Sales
            .Include(s => s.Customer)
            .Include(s => s.Lines).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(s => s.Id == saleId);

        if (sale == null) return NotFound();

        // مرتجع سابق لكل SaleLine
        var returnedByLine = await _context.SaleReturnLines
            .Where(x => x.SaleLine.SaleId == saleId && x.SaleReturn.IsActive)
            .GroupBy(x => x.SaleLineId)
            .Select(g => new { SaleLineId = g.Key, Qty = g.Sum(x => x.Qty) })
            .ToDictionaryAsync(x => x.SaleLineId, x => x.Qty);

        var vm = new SaleReturnCreateVM
        {
            SaleId = saleId,
            CustomerId = sale.CustomerId,
            CustomerName = sale.Customer?.Name ?? "",
            ReturnDate = DateTime.Now,
            Lines = sale.Lines.Select(l =>
            {
                returnedByLine.TryGetValue(l.Id, out var already);
                return new SaleReturnLineVM
                {
                    SaleLineId = l.Id,
                    ItemId = l.ItemId,
                    ItemName = l.Item?.NameAr ?? "",
                    SoldQty = l.Qty,
                    AlreadyReturnedQty = already,
                    UnitPrice = l.UnitPrice,
                    ReturnQty = 0
                };
            }).ToList()
        };

        return View("CreateForm", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateFromSale(SaleReturnCreateVM vm)
    {
        var sale = await _context.Sales
            .Include(s => s.Customer)
            .Include(s => s.Lines)
            .FirstOrDefaultAsync(s => s.Id == vm.SaleId);

        if (sale == null) return NotFound();

        var linesToReturn = vm.Lines.Where(x => x.ReturnQty > 0).ToList();
        if (linesToReturn.Count == 0)
            ModelState.AddModelError("", "أدخل كمية مرتجع في بند واحد على الأقل.");

        // رجوع سابق لكل line
        var returnedByLine = await _context.SaleReturnLines
            .Where(x => x.SaleLine.SaleId == vm.SaleId && x.SaleReturn.IsActive)
            .GroupBy(x => x.SaleLineId)
            .Select(g => new { SaleLineId = g.Key, Qty = g.Sum(x => x.Qty) })
            .ToDictionaryAsync(x => x.SaleLineId, x => x.Qty);

        // تحقق المتاح
        foreach (var r in linesToReturn)
        {
            var sl = sale.Lines.FirstOrDefault(x => x.Id == r.SaleLineId);
            if (sl == null)
            {
                ModelState.AddModelError("", "يوجد بند غير صحيح.");
                continue;
            }

            returnedByLine.TryGetValue(sl.Id, out var already);
            var available = sl.Qty - already;

            if (r.ReturnQty > available)
                ModelState.AddModelError("", $"كمية المرتجع للصنف (ItemId={sl.ItemId}) أكبر من المتاح للمرتجع.");
        }

        if (!ModelState.IsValid)
            return View("CreateForm", vm);

        using var trx = await _context.Database.BeginTransactionAsync();

        // سند المرتجع
        var ret = new SaleReturn
        {
            SaleId = sale.Id,
            CustomerId = sale.CustomerId,
            ReturnDate = vm.ReturnDate,
            Discount = vm.Discount,
            Notes = vm.Notes?.Trim(),
            IsActive = true
        };

        // نجلب allocations الأصلية للفواتير (لتحديد الدفعات التي خرجت منها)
        var saleAllocs = await _context.SaleAllocations
            .AsNoTracking()
            .Where(a => a.SaleLine.SaleId == sale.Id)
            .OrderBy(a => a.Id)
            .ToListAsync();

        // تجهيز سطور المرتجع + تخصيص دفعات (Reverse allocations)
        foreach (var r in linesToReturn)
        {
            var sl = sale.Lines.First(x => x.Id == r.SaleLineId);

            var rl = new SaleReturnLine
            {
                SaleLineId = sl.Id,
                ItemId = sl.ItemId,
                Qty = r.ReturnQty,
                UnitPrice = sl.UnitPrice,
                LineTotal = r.ReturnQty * sl.UnitPrice
            };

            // تخصيص المرتجع على نفس دفعات البيع
            var allocsForLine = saleAllocs.Where(a => a.SaleLineId == sl.Id).ToList();

            decimal qtyNeed = r.ReturnQty;

            foreach (var a in allocsForLine)
            {
                if (qtyNeed <= 0) break;

                // المتاح للرجوع من نفس batch = بيع batch - مرتجع سابق على نفس batch
                var alreadyReturnedFromBatch = await _context.SaleReturnAllocations
                    .Where(x => x.IsActive && x.SaleReturnLine.SaleLineId == sl.Id && x.BatchId == a.BatchId)
                    .SumAsync(x => x.Qty);

                var canReturnFromThisBatch = a.Qty - alreadyReturnedFromBatch;
                if (canReturnFromThisBatch <= 0) continue;

                var take = Math.Min(canReturnFromThisBatch, qtyNeed);

                rl.Allocations.Add(new SaleReturnAllocation
                {
                    BatchId = a.BatchId,
                    Qty = take,
                    IsActive = true
                });

                qtyNeed -= take;
            }

            if (qtyNeed > 0)
            {
                // لو حصلت حالة نادرة: لا يوجد ما يكفي للرجوع من نفس الدفعات
                ModelState.AddModelError("", $"تعذر توزيع مرتجع الصنف (ItemId={sl.ItemId}) على دفعات الفاتورة.");
                await trx.RollbackAsync();
                return View("CreateForm", vm);
            }

            ret.Lines.Add(rl);
        }

        ret.SubTotal = ret.Lines.Sum(x => x.LineTotal);
        ret.NetTotal = ret.SubTotal - ret.Discount;

        _context.SaleReturns.Add(ret);
        await _context.SaveChangesAsync(); // ret.Id + خطوطه

        // حركة المخزون: QtyIn حسب تخصيص الدفعات
        foreach (var line in ret.Lines)
        {
            foreach (var a in line.Allocations)
            {
                _context.StockMovements.Add(new StockMovement
                {
                    Date = ret.ReturnDate,
                    ItemId = line.ItemId,
                    BatchId = a.BatchId,
                    QtyIn = a.Qty,
                    QtyOut = 0,
                    UnitCost = 0, // (اختياري) يمكن وضع تكلفة الشراء من Batch.PurchasePrice لو تحب
                    RefType = StockRefType.SaleReturn,
                    RefId = ret.Id,
                    Notes = $"مرتجع مبيعات لفاتورة: {ret.SaleId}"
                });
            }
        }

        // ✅ قيد دفتر العميل (Credit) يقلل المديونية
        _context.CustomerLedgers.Add(new CustomerLedger
        {
            Date = ret.ReturnDate,
            CustomerId = ret.CustomerId,
            Type = CustomerLedgerType.SaleReturn,
            RefId = ret.Id,
            Debit = 0,
            Credit = ret.NetTotal,
            Notes = $"مرتجع مبيعات رقم {ret.Id} لفاتورة {ret.SaleId}",
            IsActive = true
        });

        // ✅ تحديث الفاتورة نفسها (لتبقى Remaining صحيح)
        // نقلل صافي الفاتورة بالمرتجع
        sale.SubTotal -= ret.SubTotal;
        sale.NetTotal -= ret.NetTotal;
        if (sale.SubTotal < 0) sale.SubTotal = 0;
        if (sale.NetTotal < 0) sale.NetTotal = 0;

        // Remaining = max(0, NetTotal - Paid)
        sale.RemainingAmount = Math.Max(0, sale.NetTotal - sale.PaidAmount);

        await _context.SaveChangesAsync();
        await trx.CommitAsync();

        return RedirectToAction("Details", new { id = ret.Id });
    }

    public async Task<IActionResult> Details(long id)
    {
        var ret = await _context.SaleReturns
            .AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.Sale)
            .Include(x => x.Lines).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (ret == null) return NotFound();

        var allocs = await _context.SaleReturnAllocations
            .AsNoTracking()
            .Where(a => a.SaleReturnLine.SaleReturnId == id && a.IsActive)
            .Include(a => a.Batch)
            .ToListAsync();

        ViewBag.Allocations = allocs;
        return View(ret);
    }

    public async Task<IActionResult> Print(long id)
    {
        var ret = await _context.SaleReturns
            .AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.Sale)
            .Include(x => x.Lines).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (ret == null) return NotFound();

        var allocs = await _context.SaleReturnAllocations
            .AsNoTracking()
            .Where(a => a.SaleReturnLine.SaleReturnId == id && a.IsActive)
            .Include(a => a.Batch)
            .ToListAsync();

        ViewBag.Allocations = allocs;
        return View(ret);
    }

    public async Task<IActionResult> Disable(long id)
    {
        var ret = await _context.SaleReturns
            .AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.Sale)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (ret == null) return NotFound();
        if (!ret.IsActive) return RedirectToAction(nameof(Details), new { id });

        return View(ret);
    }

    [HttpPost, ActionName("Disable")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DisableConfirmed(long id)
    {
        using var trx = await _context.Database.BeginTransactionAsync();

        // 1) جلب المرتجع (نشط فقط)
        var ret = await _context.SaleReturns
            .Include(x => x.Lines)
                .ThenInclude(l => l.Allocations.Where(a => a.IsActive))
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);

        if (ret == null) return NotFound();

        // 2) جلب الفاتورة الأصل
        var sale = await _context.Sales.FirstOrDefaultAsync(x => x.Id == ret.SaleId);
        if (sale == null) return NotFound();

        // 3) عكس المخزون: المرتجع كان QtyIn -> نعكسه QtyOut
        foreach (var line in ret.Lines)
        {
            foreach (var a in line.Allocations.Where(x => x.IsActive))
            {
                _context.StockMovements.Add(new StockMovement
                {
                    Date = DateTime.Now,
                    ItemId = line.ItemId,
                    BatchId = a.BatchId,
                    QtyIn = 0,
                    QtyOut = a.Qty,
                    UnitCost = 0,
                    RefType = StockRefType.SaleReturnDisable, // ✅ أضف enum جديد (أسفل)
                    RefId = ret.Id,
                    Notes = $"عكس مرتجع مبيعات رقم {ret.Id}"
                });

                a.IsActive = false; // تعطيل التخصيص
            }
        }

        // 4) تعطيل قيد دفتر العميل الخاص بالمرتجع
        var ledgers = await _context.CustomerLedgers
            .Where(l => l.Type == CustomerLedgerType.SaleReturn && l.RefId == ret.Id && l.IsActive)
            .ToListAsync();

        foreach (var l in ledgers)
            l.IsActive = false;

        // 5) إرجاع الفاتورة كما كانت (نرجع الصافي)
        sale.SubTotal += ret.SubTotal;
        sale.NetTotal += ret.NetTotal;

        // Remaining = max(0, Net - Paid)
        sale.RemainingAmount = Math.Max(0, sale.NetTotal - sale.PaidAmount);

        // 6) تعطيل المرتجع نفسه
        ret.IsActive = false;

        await _context.SaveChangesAsync();
        await trx.CommitAsync();

        return RedirectToAction("Details", "Sales", new { id = sale.Id });
    }
}
