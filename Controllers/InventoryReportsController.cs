using AbuAmenPharma.Data;
using AbuAmenPharma.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;

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
        return View("Stock", data);
    }

    public async Task<IActionResult> PrintPricingList()
    {
        var data = await GetPricingListDataAsync();
        return View(data);
    }

    private async Task<List<PricingListVM>> GetPricingListDataAsync()
    {
        var query = from b in _context.ItemBatches.AsNoTracking()
                    join i in _context.Items.AsNoTracking() on b.ItemId equals i.Id
                    join m in _context.Manufacturers.AsNoTracking() on i.ManufacturerId equals m.Id
                    join u in _context.Units.AsNoTracking() on i.UnitId equals u.Id
                    join mv in _context.StockMovements.AsNoTracking() on b.Id equals mv.BatchId into movs
                    where b.IsActive && i.IsActive
                    select new
                    {
                        ItemName = i.NameAr,
                        ScientificName = i.GenericName ?? "",
                        BatchNo = b.BatchNo,
                        Manufacturer = m.NameAr,
                        Unit = u.NameAr,
                        ExpiryDate = b.ExpiryDate,
                        SellPrice = b.SellPrice,
                        Balance = movs.Sum(x => x.QtyIn - x.QtyOut)
                    };

        var dataRaw = await query
            .Where(x => x.Balance > 0)
            .OrderBy(x => x.Manufacturer)
            .ThenBy(x => x.ItemName)
            .ToListAsync();

        return dataRaw.Select(x => new PricingListVM
        {
            ItemName = x.ItemName,
            ScientificName = x.ScientificName,
            BatchNo = x.BatchNo,
            Manufacturer = x.Manufacturer,
            Unit = x.Unit,
            ExpiryDate = x.ExpiryDate,
            SellPrice = x.SellPrice,
            Balance = x.Balance
        }).ToList();
    }

    public async Task<IActionResult> PricingList()
    {
        var data = await GetPricingListDataAsync();
        return View(data);
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
