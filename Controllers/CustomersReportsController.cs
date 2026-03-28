using AbuAmenPharma.Data;
using AbuAmenPharma.Models;
using AbuAmenPharma.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Admin,Operator")]
public class CustomersReportsController : Controller
{
    private readonly ApplicationDbContext _context;
    public CustomersReportsController(ApplicationDbContext context) => _context = context;

    // ✅ ملخص الأرصدة لكل العملاء
    public async Task<IActionResult> Balances()
    {
        var data = await _context.Customers
            .AsNoTracking()
            .Where(c => c.IsActive)
            .Select(c => new CustomerBalanceVM
            {
                CustomerId = c.Id,
                CustomerName = c.Name,
                Debit = _context.CustomerLedgers
                    .Where(l => l.CustomerId == c.Id && l.IsActive)
                    .Sum(l => (decimal?)l.Debit) ?? 0m,
                Credit = _context.CustomerLedgers
                    .Where(l => l.CustomerId == c.Id && l.IsActive)
                    .Sum(l => (decimal?)l.Credit) ?? 0m
            })
            .OrderByDescending(x => x.Debit - x.Credit)   // ✅ هنا
            .ToListAsync();

        return View(data);
    }

    [HttpPost]
    public async Task<IActionResult> BalancesData([FromForm] DataTablesRequest dt)
    {
        // قاعدة العملاء النشطين
        var customersQ = _context.Customers.AsNoTracking().Where(c => c.IsActive);

        // إجمالي السجلات قبل البحث
        var recordsTotal = await customersQ.CountAsync();

        // بحث
        if (!string.IsNullOrWhiteSpace(dt.search?.value))
        {
            var s = dt.search.value.Trim();
            customersQ = customersQ.Where(c => c.Name.Contains(s));
        }

        var recordsFiltered = await customersQ.CountAsync();

        // Query للأرصدة (بدون ToList هنا)
        var dataQ = customersQ.Select(c => new CustomerBalanceVM
        {
            CustomerId = c.Id,
            CustomerName = c.Name,
            Debit = _context.CustomerLedgers
                .Where(l => l.CustomerId == c.Id && l.IsActive)
                .Sum(l => (decimal?)l.Debit) ?? 0m,

            Credit = _context.CustomerLedgers
                .Where(l => l.CustomerId == c.Id && l.IsActive)
                .Sum(l => (decimal?)l.Credit) ?? 0m
        });

        // Sorting (حسب العمود المرسل من DataTables)
        var sortCol = dt.order?.FirstOrDefault()?.column ?? 0;
        var sortDir = (dt.order?.FirstOrDefault()?.dir ?? "asc").ToLower();

        // الأعمدة: 0=CustomerName, 1=Debit, 2=Credit, 3=Balance
        dataQ = (sortCol, sortDir) switch
        {
            (0, "desc") => dataQ.OrderByDescending(x => x.CustomerName),
            (0, _) => dataQ.OrderBy(x => x.CustomerName),

            (1, "desc") => dataQ.OrderByDescending(x => x.Debit),
            (1, _) => dataQ.OrderBy(x => x.Debit),

            (2, "desc") => dataQ.OrderByDescending(x => x.Credit),
            (2, _) => dataQ.OrderBy(x => x.Credit),

            (3, "desc") => dataQ.OrderByDescending(x => (x.Debit - x.Credit)),
            (3, _) => dataQ.OrderBy(x => (x.Debit - x.Credit)),

            _ => dataQ.OrderByDescending(x => (x.Debit - x.Credit))
        };

        // Paging
        var page = await dataQ
            .Skip(dt.start)
            .Take(dt.length <= 0 ? 10 : dt.length)
            .ToListAsync();

        // رجّع JSON بالشكل المطلوب
        return Json(new
        {
            draw = dt.draw,
            recordsTotal,
            recordsFiltered,
            data = page.Select(x => new
            {
                customerId = x.CustomerId,
                customerName = x.CustomerName,
                debit = x.Debit,
                credit = x.Credit,
                balance = (x.Debit - x.Credit)
            })
        });
    }

    // شاشة اختيار عميل + فترة
    public async Task<IActionResult> Statement()
    {
        ViewBag.CustomerId = new SelectList(
            await _context.Customers.Where(x => x.IsActive).OrderBy(x => x.Name).ToListAsync(),
            "Id", "Name"
        );

        // افتراضي آخر 30 يوم
        var to = DateTime.Today.AddDays(1).AddSeconds(-1);
        var from = DateTime.Today.AddDays(-30);

        var vm = new CustomerStatementVM { From = from, To = to };
        return View(vm);
    }

    // ✅ عرض كشف الحساب
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Statement(CustomerStatementVM vm)
    {
        var customer = await _context.Customers.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == vm.CustomerId && x.IsActive);

        if (customer == null)
        {
            ModelState.AddModelError("", "العميل غير موجود.");
            return await Statement(); // يرجع شاشة الاختيار
        }

        // Normalize dates
        var from = vm.From.Date;
        var to = vm.To.Date.AddDays(1).AddSeconds(-1);

        // رصيد افتتاحي = كل ما قبل from
        var opening = await _context.CustomerLedgers
            .AsNoTracking()
            .Where(x => x.CustomerId == vm.CustomerId && x.IsActive && x.Date < from)
            .Select(x => x.Debit - x.Credit)
            .SumAsync();

        // العمليات داخل الفترة
        var lines = await _context.CustomerLedgers
            .AsNoTracking()
            .Where(x => x.CustomerId == vm.CustomerId && x.IsActive && x.Date >= from && x.Date <= to)
            .OrderBy(x => x.Date)
            .ThenBy(x => x.Id)
            .ToListAsync();

        decimal running = opening;

        var lineVMs = new List<CustomerStatementLineVM>();
        foreach (var l in lines)
        {
            running += (l.Debit - l.Credit);
            lineVMs.Add(new CustomerStatementLineVM
            {
                Date = l.Date,
                TypeName = GetTypeName(l.Type),
                RefId = l.RefId,
                Notes = l.Notes,
                Debit = l.Debit,
                Credit = l.Credit,
                RunningBalance = running
            });
        }

        var totalDebit = lines.Sum(x => x.Debit);
        var totalCredit = lines.Sum(x => x.Credit);
        var closing = opening + (totalDebit - totalCredit);

        var result = new CustomerStatementVM
        {
            CustomerId = vm.CustomerId,
            CustomerName = customer.Name,
            From = from,
            To = to,
            OpeningBalance = opening,
            TotalDebit = totalDebit,
            TotalCredit = totalCredit,
            ClosingBalance = closing,
            Lines = lineVMs
        };

        ViewBag.CustomerId = new SelectList(
            await _context.Customers.Where(x => x.IsActive).OrderBy(x => x.Name).ToListAsync(),
            "Id", "Name", vm.CustomerId
        );

        return View(result);
    }

    public async Task<IActionResult> StatementPrint(int customerId, DateTime from, DateTime to)
    {
        var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == customerId && x.IsActive);

        if (customer == null)
            return NotFound();

        var lines = await _context.CustomerLedgers
            .AsNoTracking()
            .Where(l => l.CustomerId == customerId && l.IsActive)
            .OrderBy(l => l.Date)
            .ToListAsync();

        var vm = new CustomerStatementVM
        {
            CustomerId = customerId,
            CustomerName = customer.Name,
            From = from,
            To = to
        };

        // رصيد افتتاحي (قبل الفترة)
        vm.OpeningBalance = lines
            .Where(l => l.Date < from)
            .Sum(l => l.Debit - l.Credit);

        decimal running = vm.OpeningBalance;

        var periodLines = lines
            .Where(l => l.Date >= from && l.Date <= to)
            .ToList();

        foreach (var l in periodLines)
        {
            running += (l.Debit - l.Credit);

            vm.Lines.Add(new CustomerStatementLineVM
            {
                Date = l.Date,
                TypeName = l.Type.ToString(),
                RefId = l.RefId,
                Notes = l.Notes,
                Debit = l.Debit,
                Credit = l.Credit,
                RunningBalance = running
            });
        }

        vm.TotalDebit = vm.Lines.Sum(x => x.Debit);
        vm.TotalCredit = vm.Lines.Sum(x => x.Credit);
        vm.ClosingBalance = running;

        return View(vm); // سيذهب إلى StatementPrint.cshtml
    }

    [HttpGet]
    public async Task<IActionResult> SalesmanStatement()
    {
        ViewBag.SalesmanId = new SelectList(
            await _context.Salesmen.Where(x => x.IsActive).OrderBy(x => x.NameAr).ToListAsync(),
            "Id", "NameAr"
        );

        var to = DateTime.Today.AddDays(1).AddSeconds(-1);
        var from = DateTime.Today.AddDays(-30);

        return View(new SalesmanStatementVM { From = from, To = to });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SalesmanStatement(int salesmanId, DateTime from, DateTime to)
    {
        ViewBag.SalesmanId = new SelectList(
            await _context.Salesmen.Where(x => x.IsActive).OrderBy(x => x.NameAr).ToListAsync(),
            "Id", "NameAr"
        );

        var salesman = await _context.Salesmen.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == salesmanId && x.IsActive);

        if (salesman == null)
        {
            ModelState.AddModelError("", "المندوب غير موجود.");
            return View(new SalesmanStatementVM { From = from, To = to });
        }

        // عدّل أسماء الحقول حسب كيان Sale عندك (NetTotal / PaidAmount / RemainingAmount / PaymentMode...)
        var sales = await _context.Sales
            .AsNoTracking()
            .Where(s => s.SalesmanId == salesmanId && s.SaleDate >= from && s.SaleDate <= to && s.IsPosted)
            .OrderBy(s => s.SaleDate)
            .Select(s => new SalesmanStatementLineVM
            {
                Date = s.SaleDate,
                SaleId = (int)s.Id,
                CustomerName = s.Customer.Name,
                PaymentMode = s.PaymentMode.ToString(),
                NetTotal = s.NetTotal,
                PaidAmount = s.PaidAmount,
                RemainingAmount = s.RemainingAmount,
                Notes = s.Notes
            })
            .ToListAsync();

        var vm = new SalesmanStatementVM
        {
            SalesmanId = salesmanId,
            SalesmanName = salesman.NameAr,
            From = from,
            To = to,
            Lines = sales
        };

        vm.SalesCount = vm.Lines.Count;
        vm.TotalNetSales = vm.Lines.Sum(x => x.NetTotal);
        vm.TotalCash = vm.Lines.Where(x => x.PaymentMode == "Cash" || x.PaymentMode.Contains("Cash")).Sum(x => x.NetTotal);
        vm.TotalCredit = vm.Lines.Where(x => x.PaymentMode == "Credit" || x.PaymentMode.Contains("Credit")).Sum(x => x.NetTotal);

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> SalesmanStatementPrint(int salesmanId, DateTime from, DateTime to)
    {
        var salesman = await _context.Salesmen.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == salesmanId && x.IsActive);

        if (salesman == null) return NotFound();

        var sales = await _context.Sales
            .AsNoTracking()
            .Where(s => s.SalesmanId == salesmanId && s.SaleDate >= from && s.SaleDate <= to && s.IsPosted)
            .OrderBy(s => s.SaleDate)
            .Select(s => new SalesmanStatementLineVM
            {
                Date = s.SaleDate,
                SaleId = (int)s.Id,
                CustomerName = s.Customer.Name,
                PaymentMode = s.PaymentMode.ToString(),
                NetTotal = s.NetTotal,
                PaidAmount = s.PaidAmount,
                RemainingAmount = s.RemainingAmount,
                Notes = s.Notes
            })
            .ToListAsync();

        var vm = new SalesmanStatementVM
        {
            SalesmanId = salesmanId,
            SalesmanName = salesman.NameAr,
            From = from,
            To = to,
            Lines = sales
        };

        vm.SalesCount = vm.Lines.Count;
        vm.TotalNetSales = vm.Lines.Sum(x => x.NetTotal);
        vm.TotalCash = vm.Lines.Where(x => x.PaymentMode.Contains("Cash")).Sum(x => x.NetTotal);
        vm.TotalCredit = vm.Lines.Where(x => x.PaymentMode.Contains("Credit")).Sum(x => x.NetTotal);

        vm.TotalPaid = vm.Lines.Sum(x => x.PaidAmount);
        vm.TotalRemaining = vm.Lines.Sum(x => x.RemainingAmount);

        return View("SalesmanStatementPrint", vm);
    }

    private string GetTypeName(CustomerLedgerType type) => type switch
    {
        CustomerLedgerType.Sale => "فاتورة بيع",
        CustomerLedgerType.Receipt => "سند قبض",
        CustomerLedgerType.SaleReturn => "مرتجع مبيعات",
        _ => "عملية"
    };
}