using AbuAmenPharma.Data;
using AbuAmenPharma.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Admin,Operator")]
public class InventoryReportsController : Controller
{
    private readonly ApplicationDbContext _context;

    public InventoryReportsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Stock(string? q)
    {
        var data = await GetStockDataAsync(q);
        data.ForEach(d => d.IsValuationReport = false);
        ViewBag.Q = q ?? "";
        return View(data);
    }

    public async Task<IActionResult> Valuation(string? q)
    {
        var data = await GetStockDataAsync(q);
        data.ForEach(d => d.IsValuationReport = true);
        ViewBag.Q = q ?? "";
        return View("Stock", data); // Can share the same view as Stock, just handles IsValuationReport = true
    }

    private async Task<List<InventoryStockVM>> GetStockDataAsync(string? q)
    {
        var query = from b in _context.ItemBatches.AsNoTracking()
                    join i in _context.Items.AsNoTracking() on b.ItemId equals i.Id
                    join m in _context.StockMovements.AsNoTracking() on b.Id equals m.BatchId into movs
                    where b.IsActive && i.IsActive
                    select new
                    {
                        ItemId = i.Id,
                        ItemName = i.NameAr,
                        BatchId = b.Id,
                        BatchNo = b.BatchNo,
                        ExpiryDate = b.ExpiryDate,
                        PurchasePrice = b.PurchasePrice,
                        RemainingQty = movs.Sum(x => x.QtyIn - x.QtyOut)
                    };

        var dataRaw = await query
            .Where(x => string.IsNullOrWhiteSpace(q) || x.ItemName.Contains(q) || x.BatchNo.Contains(q))
            .Where(x => x.RemainingQty > 0 || x.RemainingQty < 0) // Typically want to report non-zero stock
            .OrderBy(x => x.ItemName)
            .ThenBy(x => x.ExpiryDate)
            .ThenBy(x => x.BatchNo)
            .ToListAsync();

        return dataRaw.Select(x => new InventoryStockVM
        {
            ItemId = x.ItemId,
            ItemName = x.ItemName,
            BatchId = x.BatchId,
            BatchNo = x.BatchNo,
            ExpiryDate = x.ExpiryDate,
            PurchasePrice = x.PurchasePrice,
            RemainingQty = x.RemainingQty
        }).ToList();
    }
}
