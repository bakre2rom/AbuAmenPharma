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

    // قائمة دفعات صنف محدد
    // /ItemBatches?itemId=5
    public async Task<IActionResult> Index(int itemId)
    {
        var item = await _context.Items
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == itemId && x.IsActive);

        if (item == null) return NotFound();

        // دفعات + رصيد كل دفعة
        var batches = await _context.ItemBatches
            .AsNoTracking()
            .Where(b => b.ItemId == itemId && b.IsActive)
            .OrderBy(b => b.ExpiryDate)
            .Select(b => new ItemBatchListVM
            {
                Id = b.Id,
                ItemId = b.ItemId,
                ItemNameAr = item.NameAr,
                BatchNo = b.BatchNo,
                ExpiryDate = b.ExpiryDate,
                PurchasePrice = b.PurchasePrice,
                SellPrice = b.SellPrice,
                Balance = _context.StockMovements
                    .Where(m => m.BatchId == b.Id)
                    .Select(m => m.QtyIn - m.QtyOut)
                    .Sum()
            })
            .ToListAsync();

        ViewBag.ItemId = itemId;
        ViewBag.ItemName = item.NameAr;

        return View(batches);
    }

    public async Task<IActionResult> Create(int itemId)
    {
        var item = await _context.Items
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == itemId && x.IsActive);

        if (item == null) return NotFound();

        // الحصول على آخر Batch للصنف
        var lastBatch = await _context.ItemBatches
            .Where(x => x.ItemId == itemId)
            .OrderByDescending(x => x.Id)
            .Select(x => x.BatchNo)
            .FirstOrDefaultAsync();

        int nextBatchNo = 1;

        if (!string.IsNullOrEmpty(lastBatch) && int.TryParse(lastBatch, out int lastNo))
            nextBatchNo = lastNo + 1;

        var model = new ItemBatch
        {
            ItemId = itemId,
            BatchNo = nextBatchNo.ToString(),
            ExpiryDate = DateOnly.FromDateTime(DateTime.Today.AddYears(1))
        };

        ViewBag.ItemName = item.NameAr;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ItemBatch batch)
    {
        // تأكيد أن الصنف موجود ونشط
        var item = await _context.Items.FirstOrDefaultAsync(x => x.Id == batch.ItemId && x.IsActive);
        if (item == null) return NotFound();

        // توليد رقم الدفعة مرة أخرى (لمنع التكرار)
        var lastBatch = await _context.ItemBatches
            .Where(x => x.ItemId == batch.ItemId)
            .OrderByDescending(x => x.Id)
            .Select(x => x.BatchNo)
            .FirstOrDefaultAsync();

        int nextBatchNo = 1;
        if (!string.IsNullOrEmpty(lastBatch) && int.TryParse(lastBatch, out int lastNo))
            nextBatchNo = lastNo + 1;

        batch.BatchNo = nextBatchNo.ToString();

        // Validation بسيط
        batch.BatchNo = batch.BatchNo.Trim();

        // منع تكرار نفس BatchNo لنفس الصنف (مع IsActive)
        var exists = await _context.ItemBatches.AnyAsync(x =>
            x.ItemId == batch.ItemId &&
            x.BatchNo == batch.BatchNo &&
            x.IsActive);

        if (exists)
            ModelState.AddModelError(nameof(batch.BatchNo), "رقم الدفعة موجود مسبقاً لهذا الصنف.");

        if (!ModelState.IsValid)
        {
            ViewBag.ItemName = item.NameAr;
            return View(batch);
        }

        _context.ItemBatches.Add(batch);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { itemId = batch.ItemId });
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

        // منع التكرار (لو تغير الرقم)
        var exists = await _context.ItemBatches.AnyAsync(x =>
            x.Id != id &&
            x.ItemId == db.ItemId &&
            x.BatchNo == batch.BatchNo &&
            x.IsActive);

        if (exists)
            ModelState.AddModelError(nameof(batch.BatchNo), "رقم الدفعة موجود مسبقاً لهذا الصنف.");

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
        return RedirectToAction(nameof(Index), new { itemId = db.ItemId });
    }

    public async Task<IActionResult> Disable(int? id)
    {
        if (id == null) return NotFound();

        var batch = await _context.ItemBatches
            .Include(b => b.Item)
            .FirstOrDefaultAsync(b => b.Id == id && b.IsActive);

        if (batch == null) return NotFound();

        // لو عليها رصيد لاحقًا ممكن نمنع التعطيل - الآن نخليها عادي
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
            batch.IsActive = false;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { itemId = batch.ItemId });
        }
        return RedirectToAction("Index", "Items");
    }
}