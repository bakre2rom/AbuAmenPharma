using AbuAmenPharma.Data;
using AbuAmenPharma.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

public class SalesReportsController : Controller
{
    private readonly ApplicationDbContext _context;
    public SalesReportsController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Aging()
    {
        ViewBag.CustomerId = new SelectList(
            await _context.Customers.Where(x => x.IsActive).OrderBy(x => x.Name).ToListAsync(),
            "Id", "Name"
        );

        return View(); // بدون Model
    }

    [HttpPost]
    public async Task<IActionResult> AgingData([FromForm] DataTablesRequest dt, [FromForm] int? customerId)
    {
        var today = DateTime.Today;

        // base query
        var q = _context.Sales
            .AsNoTracking()
            .Include(s => s.Customer)
            .Where(s => s.IsPosted && s.RemainingAmount > 0);

        if (customerId.HasValue)
            q = q.Where(s => s.CustomerId == customerId.Value);

        // Search (على اسم العميل أو رقم الفاتورة)
        if (!string.IsNullOrWhiteSpace(dt.search?.value))
        {
            var s = dt.search.value.Trim();
            q = q.Where(x => x.Customer!.Name.Contains(s) || x.Id.ToString().Contains(s));
        }

        var recordsTotal = await q.CountAsync();
        var recordsFiltered = recordsTotal; // لأننا طبقنا البحث على نفس q

        // Projection (احسب Days + Buckets)
        var dataQ = q.Select(s => new
        {
            saleId = s.Id,
            customerName = s.Customer!.Name,
            saleDate = s.SaleDate,
            remaining = s.RemainingAmount,
            paid = s.PaidAmount,
            netTotal = s.NetTotal,
            days = EF.Functions.DateDiffDay(s.SaleDate.Date, today),

            b0_30 = EF.Functions.DateDiffDay(s.SaleDate.Date, today) <= 30 ? s.RemainingAmount : 0,
            b31_60 = (EF.Functions.DateDiffDay(s.SaleDate.Date, today) > 30 && EF.Functions.DateDiffDay(s.SaleDate.Date, today) <= 60) ? s.RemainingAmount : 0,
            b61_90 = (EF.Functions.DateDiffDay(s.SaleDate.Date, today) > 60 && EF.Functions.DateDiffDay(s.SaleDate.Date, today) <= 90) ? s.RemainingAmount : 0,
            b90p = EF.Functions.DateDiffDay(s.SaleDate.Date, today) > 90 ? s.RemainingAmount : 0
        });

        // Sorting
        var sortCol = dt.order?.FirstOrDefault()?.column ?? 3;
        var sortDir = (dt.order?.FirstOrDefault()?.dir ?? "desc").ToLower();

        // الأعمدة: 0=SaleId, 1=Customer, 2=Date, 3=Days, 4=Remaining, 5=0-30, 6=31-60, 7=61-90, 8=90+
        dataQ = (sortCol, sortDir) switch
        {
            (0, "asc") => dataQ.OrderBy(x => x.saleId),
            (0, _) => dataQ.OrderByDescending(x => x.saleId),

            (1, "asc") => dataQ.OrderBy(x => x.customerName),
            (1, _) => dataQ.OrderByDescending(x => x.customerName),

            (2, "asc") => dataQ.OrderBy(x => x.saleDate),
            (2, _) => dataQ.OrderByDescending(x => x.saleDate),

            (3, "asc") => dataQ.OrderBy(x => x.days),
            (3, _) => dataQ.OrderByDescending(x => x.days),

            (4, "asc") => dataQ.OrderBy(x => x.remaining),
            (4, _) => dataQ.OrderByDescending(x => x.remaining),

            _ => dataQ.OrderByDescending(x => x.days)
        };

        // paging
        var page = await dataQ
            .Skip(dt.start)
            .Take(dt.length <= 0 ? 25 : dt.length)
            .ToListAsync();

        return Json(new
        {
            draw = dt.draw,
            recordsTotal,
            recordsFiltered,
            data = page
        });
    }

    //public async Task<IActionResult> Aging(int? customerId)
    //{
    //    ViewBag.CustomerId = new SelectList(
    //        await _context.Customers.Where(x => x.IsActive).OrderBy(x => x.Name).ToListAsync(),
    //        "Id", "Name", customerId
    //    );

    //    var today = DateTime.Today;

    //    var query = _context.Sales
    //        .AsNoTracking()
    //        .Include(s => s.Customer)
    //        .Where(s => s.IsPosted && s.RemainingAmount > 0);

    //    if (customerId.HasValue)
    //        query = query.Where(s => s.CustomerId == customerId);

    //    var sales = await query.ToListAsync();

    //    var result = sales.Select(s =>
    //    {
    //        var days = (today - s.SaleDate.Date).Days;

    //        var vm = new SalesAgingVM
    //        {
    //            SaleId = s.Id,
    //            CustomerName = s.Customer!.Name,
    //            SaleDate = s.SaleDate,
    //            NetTotal = s.NetTotal,
    //            Paid = s.PaidAmount,
    //            Remaining = s.RemainingAmount,
    //            Days = days
    //        };

    //        if (days <= 30) vm.Bucket_0_30 = s.RemainingAmount;
    //        else if (days <= 60) vm.Bucket_31_60 = s.RemainingAmount;
    //        else if (days <= 90) vm.Bucket_61_90 = s.RemainingAmount;
    //        else vm.Bucket_90Plus = s.RemainingAmount;

    //        return vm;
    //    })
    //    .OrderByDescending(x => x.Days)
    //    .ToList();

    //    return View(result);
    //}
}