using AbuAmenPharma.Data;
using AbuAmenPharma.Models;
using AbuAmenPharma.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Admin,Operator")]
public class ItemBatchesController : Controller
{
    private readonly ApplicationDbContext _context;
    public ItemBatchesController(ApplicationDbContext context) => _context = context;

    [HttpGet("/ItemBatches/Index.cshtml")]
    public IActionResult LegacyIndex(int? itemId)
        => RedirectToAction(nameof(Index), new { itemId });

    // /ItemBatches?itemId=5
    // If itemId is omitted, show all active batches.
    public async Task<IActionResult> Index(int? itemId)
    {
        var batchesQuery = _context.ItemBatches
            .AsNoTracking()
            .Where(b => b.IsActive);

        string viewTitle;
        if (itemId.HasValue)
        {
            var item = await _context.Items
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == itemId.Value && x.IsActive);

            if (item == null) return NotFound();

            batchesQuery = batchesQuery.Where(b => b.ItemId == itemId.Value);
            viewTitle = $"دفعات الصنف: {item.NameAr}";
            ViewBag.ItemId = item.Id;
            ViewBag.ItemName = item.NameAr;
        }
        else
        {
            batchesQuery = batchesQuery.Where(b => b.Item != null && b.Item.IsActive);
            viewTitle = "إدارة الدفعات";
            ViewBag.ItemId = null;
            ViewBag.ItemName = "كل الدفعات";
        }

        // دفعات + رصيد كل دفعة
        var batches = await batchesQuery
            .OrderBy(b => b.ExpiryDate)
            .ThenBy(b => b.Item!.NameAr)
            .ThenBy(b => b.BatchNo)
            .Select(b => new ItemBatchListVM
            {
                Id = b.Id,
                ItemId = b.ItemId,
                ItemNameAr = b.Item!.NameAr,
                BatchNo = b.BatchNo,
                ExpiryDate = b.ExpiryDate,
                PurchasePrice = b.PurchasePrice,
                SellPrice = b.SellPrice,
                Balance = _context.StockMovements
                    .Where(m => m.BatchId == b.Id)
                    .Select(m => (decimal?)(m.QtyIn - m.QtyOut))
                    .Sum() ?? 0m
            })
            .ToListAsync();

        ViewData["Title"] = viewTitle;
        ViewBag.HasItemFilter = itemId.HasValue;
        return View(batches);
    }

    public async Task<IActionResult> Create(int itemId)
    {
        var itemExists = await _context.Items
            .AsNoTracking()
            .AnyAsync(x => x.Id == itemId && x.IsActive);

        if (!itemExists) return NotFound();

        TempData["ErrorMessage"] = "أفضل ممارسة: إنشاء الدفعة يتم من خلال فاتورة شراء/توريد فقط، وليس من شاشة الدفعات مباشرة.";
        return RedirectToAction("Create", "Purchases");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ItemBatch batch)
    {
        var itemExists = await _context.Items
            .AsNoTracking()
            .AnyAsync(x => x.Id == batch.ItemId && x.IsActive);

        if (!itemExists) return NotFound();

        TempData["ErrorMessage"] = "تم إيقاف إنشاء الدفعات يدويًا. أنشئ الدفعة عبر فاتورة شراء لضمان ترحيل المخزون والمحاسبة بشكل صحيح.";
        return RedirectToAction("Create", "Purchases");
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var batch = await _context.ItemBatches
            .Include(b => b.Item)
            .FirstOrDefaultAsync(b => b.Id == id && b.IsActive);

        if (batch == null) return NotFound();

        ViewBag.ItemName = batch.Item!.NameAr;
        return View(batch);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ItemBatch batch)
    {
        if (id != batch.Id) return NotFound();

        var db = await _context.ItemBatches
            .Include(b => b.Item)
            .FirstOrDefaultAsync(b => b.Id == id && b.IsActive);

        if (db == null) return NotFound();

        batch.BatchNo = batch.BatchNo.Trim();
        var hasMovements = await _context.StockMovements
            .AsNoTracking()
            .AnyAsync(m => m.BatchId == id);

        // منع التكرار (لو تغير الرقم)
        var exists = await _context.ItemBatches.AnyAsync(x =>
            x.Id != id &&
            x.ItemId == db.ItemId &&
            x.BatchNo == batch.BatchNo &&
            x.IsActive);

        if (exists)
            ModelState.AddModelError(nameof(batch.BatchNo), "رقم الدفعة موجود مسبقاً لهذا الصنف.");

        if (hasMovements && batch.PurchasePrice != db.PurchasePrice)
            ModelState.AddModelError(nameof(batch.PurchasePrice), "لا يمكن تعديل سعر الشراء بعد وجود حركات مخزون على الدفعة.");

        if (!ModelState.IsValid)
        {
            ViewBag.ItemName = db.Item!.NameAr;
            return View(batch);
        }

        db.BatchNo = batch.BatchNo;
        db.ExpiryDate = batch.ExpiryDate;
        db.PurchasePrice = batch.PurchasePrice;
        db.SellPrice = batch.SellPrice;

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "تمت العملية بنجاح";
        return RedirectToAction(nameof(Index), new { itemId = db.ItemId });
    }

    public async Task<IActionResult> Disable(int? id)
    {
        if (id == null) return NotFound();

        var batch = await _context.ItemBatches
            .Include(b => b.Item)
            .FirstOrDefaultAsync(b => b.Id == id && b.IsActive);

        if (batch == null) return NotFound();

        var balance = await GetBatchBalanceAsync(batch.Id);
        if (balance > 0)
        {
            TempData["ErrorMessage"] = $"لا يمكن تعطيل الدفعة ({batch.BatchNo}) لأن عليها رصيد {balance:N2}.";
            return RedirectToAction(nameof(Index), new { itemId = batch.ItemId });
        }

        ViewBag.ItemName = batch.Item!.NameAr;
        return View(batch);
    }

    [HttpPost, ActionName("Disable")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DisableConfirmed(int id)
    {
        var batch = await _context.ItemBatches.FindAsync(id);
        if (batch != null)
        {
            var balance = await GetBatchBalanceAsync(batch.Id);
            if (balance > 0)
            {
                TempData["ErrorMessage"] = $"لا يمكن تعطيل الدفعة ({batch.BatchNo}) لأن عليها رصيد {balance:N2}.";
                return RedirectToAction(nameof(Index), new { itemId = batch.ItemId });
            }

            batch.IsActive = false;
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "تمت العملية بنجاح";
            return RedirectToAction(nameof(Index), new { itemId = batch.ItemId });
        }
        return RedirectToAction("Index", "Items");
    }

    private async Task<decimal> GetBatchBalanceAsync(int batchId)
        => await _context.StockMovements
            .Where(m => m.BatchId == batchId)
            .Select(m => m.QtyIn - m.QtyOut)
            .SumAsync();
}
