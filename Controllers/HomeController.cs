using AbuAmenPharma.Data;
using AbuAmenPharma.Models;
using AbuAmenPharma.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace AbuAmenPharma.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var soonDays = 30;

            // مبيعات اليوم
            var salesTodayQ = _context.Sales.AsNoTracking()
                .Where(s => s.IsPosted && s.SaleDate >= today && s.SaleDate < today.AddDays(1));

            // مبيعات الشهر
            var salesMonthQ = _context.Sales.AsNoTracking()
                .Where(s => s.IsPosted && s.SaleDate >= monthStart && s.SaleDate < monthStart.AddMonths(1));

            // غير مسدد
            var unpaidQ = _context.Sales.AsNoTracking()
                .Where(s => s.IsPosted && s.RemainingAmount > 0);

            // تحصيل اليوم
            var receiptsTodayQ = _context.CustomerReceipts.AsNoTracking()
                .Where(r => r.IsActive && r.Date >= today && r.Date < today.AddDays(1));

            // ✅ أرصدة الدفعات (GroupBy أولاً)
            var batchBalancesQ = _context.StockMovements.AsNoTracking()
                .GroupBy(m => m.BatchId)
                .Select(g => new
                {
                    BatchId = g.Key,
                    Balance = g.Sum(x => x.QtyIn - x.QtyOut)
                });

            // تنبيه انتهاء (دفعات برصيد > 0)
            var todayDO = DateOnly.FromDateTime(today);
            var soonDO = todayDO.AddDays(soonDays);

            var activeBatchesQ =
                from b in _context.ItemBatches.AsNoTracking()
                join bb in batchBalancesQ on b.Id equals bb.BatchId
                where bb.Balance > 0 && b.ExpiryDate != null
                select new { b.ExpiryDate, bb.Balance };

            // ✅ أرصدة الأصناف (GroupBy على ItemId مباشرة)
            var itemBalancesQ = _context.StockMovements.AsNoTracking()
                .GroupBy(m => m.ItemId)
                .Select(g => new
                {
                    ItemId = g.Key,
                    Balance = g.Sum(x => x.QtyIn - x.QtyOut)
                });

            // Top 5 items sold this month
            var topItems = await _context.SaleAllocations
                .AsNoTracking()
                .Where(a => a.SaleLine.Sale.IsPosted &&
                            a.SaleLine.Sale.SaleDate >= monthStart &&
                            a.SaleLine.Sale.SaleDate < monthStart.AddMonths(1))
                .GroupBy(a => new { a.SaleLine.ItemId, a.SaleLine.Item.NameAr })
                .Select(g => new TopItemVM
                {
                    ItemId = g.Key.ItemId,
                    ItemName = g.Key.NameAr,
                    Qty = g.Sum(x => x.Qty)
                })
                .OrderByDescending(x => x.Qty)
                .Take(5)
                .ToListAsync();

            // ✅ OutOfStock: الأصناف النشطة التي لا يوجد لها رصيد أو رصيدها <=0
            // نجلب balances في Dictionary ثم نعدّها مع Items
            var itemBalancesDict = await itemBalancesQ.ToDictionaryAsync(x => x.ItemId, x => x.Balance);
            var activeItemIds = await _context.Items.AsNoTracking()
                .Where(i => i.IsActive)
                .Select(i => i.Id)
                .ToListAsync();

            var outOfStockCount = activeItemIds.Count(id => !itemBalancesDict.ContainsKey(id) || itemBalancesDict[id] <= 0);

            var vm = new HomeDashboardVM
            {
                SalesTodayCount = await salesTodayQ.CountAsync(),
                SalesTodayNet = await salesTodayQ.SumAsync(x => (decimal?)x.NetTotal) ?? 0m,

                SalesMonthNet = await salesMonthQ.SumAsync(x => (decimal?)x.NetTotal) ?? 0m,

                UnpaidInvoicesCount = await unpaidQ.CountAsync(),
                UnpaidInvoicesRemaining = await unpaidQ.SumAsync(x => (decimal?)x.RemainingAmount) ?? 0m,

                ReceiptsTodayTotal = await receiptsTodayQ.SumAsync(x => (decimal?)x.Amount) ?? 0m,

                ExpiredBatchesCount = await activeBatchesQ.CountAsync(x => x.ExpiryDate < todayDO),
                ExpiringSoonBatchesCount = await activeBatchesQ.CountAsync(x => x.ExpiryDate >= todayDO && x.ExpiryDate <= soonDO),

                OutOfStockItemsCount = outOfStockCount,

                TopItems = topItems
            };

            return View(vm);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
