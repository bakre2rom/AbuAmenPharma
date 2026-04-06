using AbuAmenPharma.Data;
using AbuAmenPharma.Models;
using AbuAmenPharma.ViewModels;
using AbuAmenPharma.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Admin,Operator")]
public class StockReportsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IStockService _stockService;

    public StockReportsController(ApplicationDbContext context, IStockService stockService)
    {
        _context = context;
        _stockService = stockService;
    }

    // 1) رصيد حسب الدفعة (كل الدفعات اللي رصيدها > 0)
    public async Task<IActionResult> StockByBatch(string? q)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        var query =
            from b in _context.ItemBatches.AsNoTracking()
            join i in _context.Items.AsNoTracking() on b.ItemId equals i.Id
            join m in _context.StockMovements.AsNoTracking() on b.Id equals m.BatchId into movs
            select new
            {
                b.Id,
                b.ItemId,
                ItemName = i.NameAr,
                b.BatchNo,
                b.ExpiryDate,
                Balance = movs.Sum(x => x.QtyIn - x.QtyOut)
            };

        var data = await query
            .Where(x => x.Balance > 0)
            .Where(x => string.IsNullOrWhiteSpace(q) || x.ItemName.Contains(q) || x.BatchNo.Contains(q))
            .OrderBy(x => x.ItemName).ThenBy(x => x.ExpiryDate).ThenBy(x => x.Id)
            .Select(x => new BatchStockVM
            {
                BatchId = x.Id,
                ItemId = x.ItemId,
                ItemName = x.ItemName,
                BatchNo = x.BatchNo,
                ExpiryDate = x.ExpiryDate,
                Balance = x.Balance,
                DaysToExpiry = x.ExpiryDate.DayNumber - today.DayNumber
            })
            .ToListAsync();

        ViewBag.Q = q ?? "";
        return View(data);
    }

    // 2) قرب الانتهاء/منتهي (افتراضي 90 يوم)
    public async Task<IActionResult> ExpiryAlert(int days = 90, string? q = null)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var max = today.AddDays(days);

        var query =
            from b in _context.ItemBatches.AsNoTracking()
            join i in _context.Items.AsNoTracking() on b.ItemId equals i.Id
            join m in _context.StockMovements.AsNoTracking() on b.Id equals m.BatchId into movs
            select new
            {
                b.Id,
                b.ItemId,
                ItemName = i.NameAr,
                b.BatchNo,
                b.ExpiryDate,
                Balance = movs.Sum(x => x.QtyIn - x.QtyOut)
            };

        var data = await query
            .Where(x => x.Balance > 0)
            .Where(x => x.ExpiryDate <= max) // قريب/منتهي
            .Where(x => string.IsNullOrWhiteSpace(q) || x.ItemName.Contains(q) || x.BatchNo.Contains(q))
            .OrderBy(x => x.ExpiryDate).ThenBy(x => x.ItemName)
            .Select(x => new BatchStockVM
            {
                BatchId = x.Id,
                ItemId = x.ItemId,
                ItemName = x.ItemName,
                BatchNo = x.BatchNo,
                ExpiryDate = x.ExpiryDate,
                Balance = x.Balance,
                DaysToExpiry = x.ExpiryDate.DayNumber - today.DayNumber
            })
            .ToListAsync();

        ViewBag.Days = days;
        ViewBag.Q = q ?? "";
        return View(data);
    }

    public async Task<IActionResult> ItemMovement(int? itemId, DateTime? from, DateTime? to, StockRefType? refType)
    {
        ViewBag.Items = await _context.Items
            .Where(x => x.IsActive)
            .OrderBy(x => x.NameAr)
            .ToListAsync();

        ViewBag.ItemId = itemId;
        ViewBag.From = from?.ToString("yyyy-MM-dd");
        ViewBag.To = to?.ToString("yyyy-MM-dd");
        ViewBag.RefType = refType;

        if (itemId == null)
            return View(new List<ItemMovementVM>());

        var query = _context.StockMovements
            .Include(x => x.Batch)
            .Where(x => x.ItemId == itemId);

        if (from.HasValue)
            query = query.Where(x => x.Date >= from.Value);

        if (to.HasValue)
            query = query.Where(x => x.Date <= to.Value);

        if (refType.HasValue)
            query = query.Where(x => x.RefType == refType.Value);

        var movements = await query
            .OrderBy(x => x.Date)
            .ThenBy(x => x.Id)
            .ToListAsync();

        decimal balance = 0;
        var result = new List<ItemMovementVM>();

        foreach (var m in movements)
        {
            balance += (m.QtyIn - m.QtyOut);

            result.Add(new ItemMovementVM
            {
                Date = m.Date,
                RefType = GetRefTypeAr(m.RefType),
                RefId = m.RefId,
                BatchNo = m.Batch?.BatchNo,
                QtyIn = m.QtyIn,
                QtyOut = m.QtyOut,
                BalanceAfter = balance
            });
        }

        return View(result);
    }

    private string GetRefTypeAr(StockRefType type)
    {
        return type switch
        {
            StockRefType.Purchase => "مشتريات",
            StockRefType.PurchaseReturn => "مرتجع مشتريات",
            StockRefType.Sale => "مبيعات",
            StockRefType.SaleReturn => "مرتجع مبيعات",
            StockRefType.Adjust => "تسوية جردية",
            StockRefType.SaleReturnDisable => "إلغاء مرتجع مبيعات",
            _ => type.ToString()
        };
    }

    public async Task<IActionResult> Inventory(int days = 90, string? q = null)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        var query =
            from b in _context.ItemBatches.AsNoTracking()
            join i in _context.Items.AsNoTracking() on b.ItemId equals i.Id
            join m in _context.StockMovements.AsNoTracking() on b.Id equals m.BatchId into movs
            select new
            {
                b.Id,
                b.ItemId,
                ItemName = i.NameAr,
                b.BatchNo,
                b.ExpiryDate,
                b.PurchasePrice,
                Balance = movs.Sum(x => x.QtyIn - x.QtyOut)
            };

        var dataRaw = await query
            .Where(x => x.Balance > 0)
            .Where(x => string.IsNullOrWhiteSpace(q) || x.ItemName.Contains(q) || x.BatchNo.Contains(q))
            .OrderBy(x => x.ItemName)
            .ThenBy(x => x.ExpiryDate)
            .ToListAsync();

        var data = dataRaw.Select(x =>
        {
            int d = x.ExpiryDate.DayNumber - today.DayNumber;
            string status = d < 0 ? "منتهي" : (d <= days ? "قريب" : "سليم");

            return new InventoryStocktakeVM
            {
                ItemId = x.ItemId,
                ItemName = x.ItemName,
                BatchId = x.Id,
                BatchNo = x.BatchNo,
                ExpiryDate = x.ExpiryDate,
                Balance = x.Balance,
                DaysToExpiry = d,
                Status = status,
                Cost = x.PurchasePrice 
            };
        }).ToList();

        ViewBag.TotalValue = data.Sum(x => x.Total);
        ViewBag.Days = days;
        ViewBag.Q = q ?? "";
        return View(data);
    }
}
