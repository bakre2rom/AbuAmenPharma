using AbuAmenPharma.Data;
using AbuAmenPharma.Models;
using AbuAmenPharma.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Admin,Operator")]
public class PurchaseReturnsController : Controller
{
    private readonly ApplicationDbContext _context;
    public PurchaseReturnsController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index()
    {
        var data = await _context.PurchaseReturns
            .AsNoTracking()
            .Include(x => x.Supplier)
            .Include(x => x.Purchase)
            .OrderByDescending(x => x.Id)
            .Take(200)
            .ToListAsync();

        return View(data);
    }

    public async Task<IActionResult> Details(long id)
    {
        var ret = await _context.PurchaseReturns
            .AsNoTracking()
            .Include(x => x.Supplier)
            .Include(x => x.Purchase)
            .Include(x => x.Lines)
                .ThenInclude(l => l.Item)
            .Include(x => x.Lines)
                .ThenInclude(l => l.Batch)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (ret == null) return NotFound();

        return View(ret);
    }

    public async Task<IActionResult> Print(long id)
    {
        var ret = await _context.PurchaseReturns
            .AsNoTracking()
            .Include(x => x.Supplier)
            .Include(x => x.Purchase)
            .Include(x => x.Lines).ThenInclude(l => l.Item)
            .Include(x => x.Lines).ThenInclude(l => l.Batch)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (ret == null) return NotFound();

        return View(ret);
    }

    // شاشة اختيار فاتورة شراء (بسيطة)
    public async Task<IActionResult> Create()
    {
        // نعرض آخر 200 فاتورة شراء لاختيار واحدة
        var purchases = await _context.Purchases
            .AsNoTracking()
            .Include(x => x.Supplier)
            .OrderByDescending(x => x.Id)
            .Take(200)
            .ToListAsync();

        return View(purchases);
    }

    // تحميل بنود فاتورة الشراء لعمل مرتجع
    public async Task<IActionResult> CreateFromPurchase(long purchaseId)
    {
        var purchase = await _context.Purchases
            .Include(x => x.Supplier)
            .Include(x => x.Lines)
                .ThenInclude(l => l.Item)
            .Include(x => x.Lines)
                .ThenInclude(l => l.Batch)
            .FirstOrDefaultAsync(x => x.Id == purchaseId);

        if (purchase == null) return NotFound();

        // كمية المرتجعات السابقة لكل PurchaseLine
        var returnedByLine = await _context.PurchaseReturnLines
            .Where(x => x.PurchaseLine!.PurchaseId == purchaseId)
            .GroupBy(x => x.PurchaseLineId)
            .Select(g => new { PurchaseLineId = g.Key, Qty = g.Sum(x => x.Qty) })
            .ToDictionaryAsync(x => x.PurchaseLineId, x => x.Qty);

        var vm = new PurchaseReturnCreateVM
        {
            PurchaseId = purchaseId,
            SupplierId = purchase.SupplierId,
            SupplierName = purchase.Supplier?.Name ?? "",
            ReturnDate = DateTime.Now,
            Lines = purchase.Lines.Select(l =>
            {
                returnedByLine.TryGetValue(l.Id, out var already);
                return new PurchaseReturnLineVM
                {
                    PurchaseLineId = l.Id,
                    ItemId = l.ItemId,
                    BatchId = l.BatchId,
                    ItemName = l.Item?.NameAr ?? "",
                    BatchNo = l.Batch?.BatchNo ?? "-",
                    ExpiryDate = l.Batch?.ExpiryDate,
                    PurchasedQty = l.Qty,
                    AlreadyReturnedQty = already,
                    UnitCost = l.UnitCost,
                    ReturnQty = 0
                };
            }).ToList()
        };

        return View("CreateForm", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateFromPurchase(PurchaseReturnCreateVM vm)
    {
        var purchase = await _context.Purchases
            .Include(x => x.Supplier)
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == vm.PurchaseId);

        if (purchase == null) return NotFound();

        var linesToReturn = vm.Lines?.Where(x => x.ReturnQty > 0).ToList() ?? new();
        if (linesToReturn.Count == 0)
            ModelState.AddModelError("", "أدخل كمية مرتجع في بند واحد على الأقل.");

        // ✅ المرتجع السابق لكل PurchaseLine (نشط فقط)
        var returnedByLine = await _context.PurchaseReturnLines
            .Where(x => x.PurchaseLine != null && x.PurchaseLine.PurchaseId == vm.PurchaseId)
            .GroupBy(x => x.PurchaseLineId)
            .Select(g => new { PurchaseLineId = g.Key, Qty = g.Sum(x => x.Qty) })
            .ToDictionaryAsync(x => x.PurchaseLineId, x => x.Qty);

        // ✅ تحقق من المتاح للمرتجع من الفاتورة + تحقق BatchId لمقاومة التلاعب
        foreach (var r in linesToReturn)
        {
            var pl = purchase.Lines.FirstOrDefault(x => x.Id == r.PurchaseLineId);
            if (pl == null)
            {
                ModelState.AddModelError("", "يوجد بند غير صحيح داخل المرتجع.");
                continue;
            }

            // حماية: BatchId في POST يجب يطابق BatchId الحقيقي في سطر الشراء
            if (r.BatchId != pl.BatchId)
                ModelState.AddModelError("", "حدث تلاعب في بيانات الدفعة (Batch).");

            returnedByLine.TryGetValue(pl.Id, out var alreadyReturned);
            var availableToReturn = pl.Qty - alreadyReturned;

            if (r.ReturnQty > availableToReturn)
                ModelState.AddModelError("", $"كمية المرتجع أكبر من المتاح من الفاتورة للصنف (ItemId={pl.ItemId}).");
        }

        // ✅ تحقق من رصيد المخزون لكل Batch (مرة واحدة لكل Batch)
        var requestedByBatch = linesToReturn
            .GroupBy(x => x.BatchId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.ReturnQty));

        foreach (var kv in requestedByBatch)
        {
            var batchId = kv.Key;
            var requestedQty = kv.Value;

            var balance = await GetBatchBalanceAsync(batchId);
            if (requestedQty > balance)
                ModelState.AddModelError("", $"لا يمكن إرجاع {requestedQty} من الدفعة ({batchId}) لأن الرصيد {balance} فقط.");
        }

        if (!ModelState.IsValid)
            return View("CreateForm", vm);

        using var trx = await _context.Database.BeginTransactionAsync();

        try
        {
            var ret = new PurchaseReturn
            {
                PurchaseId = vm.PurchaseId,
                SupplierId = purchase.SupplierId,
                ReturnDate = vm.ReturnDate,
                Notes = vm.Notes?.Trim()
            };

            foreach (var r in linesToReturn)
            {
                var pl = purchase.Lines.First(x => x.Id == r.PurchaseLineId);

                returnedByLine.TryGetValue(pl.Id, out var alreadyReturned);
                var availableToReturn = pl.Qty - alreadyReturned;

                if (r.ReturnQty <= 0 || r.ReturnQty > availableToReturn)
                    continue;

                ret.Lines.Add(new PurchaseReturnLine
                {
                    PurchaseLineId = pl.Id,
                    ItemId = pl.ItemId,
                    BatchId = pl.BatchId,  // ✅ مصدر الحقيقة
                    Qty = r.ReturnQty,
                    UnitCost = pl.UnitCost,
                    LineTotal = r.ReturnQty * pl.UnitCost
                });
            }

            if (ret.Lines.Count == 0)
            {
                ModelState.AddModelError("", "لا توجد بنود صحيحة للمرتجع.");
                await trx.RollbackAsync();
                return View("CreateForm", vm);
            }

            ret.Total = ret.Lines.Sum(x => x.LineTotal);

            _context.PurchaseReturns.Add(ret);
            await _context.SaveChangesAsync(); // ret.Id

            foreach (var l in ret.Lines)
            {
                _context.StockMovements.Add(new StockMovement
                {
                    Date = ret.ReturnDate,
                    ItemId = l.ItemId,
                    BatchId = l.BatchId,
                    QtyIn = 0,
                    QtyOut = l.Qty,
                    UnitCost = l.UnitCost,
                    RefType = StockRefType.PurchaseReturn,
                    RefId = ret.Id,
                    Notes = $"مرتجع مشتريات لفاتورة: {ret.PurchaseId}"
                });
            }

            await _context.SaveChangesAsync();
            await trx.CommitAsync();

            TempData["SuccessMessage"] = "تمت العملية بنجاح";
            return RedirectToAction(nameof(Index));
        }
        catch
        {
            await trx.RollbackAsync();
            throw;
        }
    }

    private async Task<decimal> GetBatchBalanceAsync(int batchId)
    {
        return await _context.StockMovements
            .Where(m => m.BatchId == batchId)
            .Select(m => m.QtyIn - m.QtyOut)
            .SumAsync();
    }
}
