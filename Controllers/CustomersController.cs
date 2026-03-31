using AbuAmenPharma.Data;
using AbuAmenPharma.Models;
using AbuAmenPharma.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Admin,Operator")]
public class CustomersController : Controller
{
    private readonly ApplicationDbContext _context;
    public CustomersController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index()
    {
        var customers = await _context.Customers
            .AsNoTracking()
            .Where(x => x.IsActive)
            .Include(x => x.Salesman)
            .OrderBy(x => x.Name)
            .ToListAsync();

        ViewBag.SalesmanId = new SelectList(
            await _context.Salesmen.Where(x => x.IsActive).OrderBy(x => x.NameAr).ToListAsync(),
            "Id",
            "NameAr"
        );

        return View(customers);
    }

    public async Task<IActionResult> Details(int id)
    {
        var customer = await _context.Customers
            .AsNoTracking()
            .Include(x => x.Salesman)
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);

        if (customer == null) return NotFound();

        var ledgers = await _context.CustomerLedgers
            .AsNoTracking()
            .Where(x => x.CustomerId == id && x.IsActive)
            .OrderBy(x => x.Date)
            .ThenBy(x => x.Id)
            .ToListAsync();

        var receiptIds = ledgers
            .Where(x => x.Type == CustomerLedgerType.Receipt)
            .Select(x => x.RefId)
            .Distinct()
            .ToList();

        var saleReturnIds = ledgers
            .Where(x => x.Type == CustomerLedgerType.SaleReturn)
            .Select(x => x.RefId)
            .Distinct()
            .ToList();

        var receiptAllocations = await _context.CustomerReceiptAllocations
            .AsNoTracking()
            .Where(x => x.IsActive && receiptIds.Contains(x.ReceiptId))
            .Select(x => new { x.ReceiptId, x.SaleId })
            .ToListAsync();

        var receiptSalesMap = receiptAllocations
            .GroupBy(x => x.ReceiptId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.SaleId).Distinct().OrderBy(x => x).ToList()
            );

        var saleReturnMap = await _context.SaleReturns
            .AsNoTracking()
            .Where(x => saleReturnIds.Contains(x.Id))
            .Select(x => new { x.Id, x.SaleId })
            .ToDictionaryAsync(x => x.Id, x => x.SaleId);

        decimal running = 0m;
        var lines = new List<CustomerLedgerLineVM>();
        foreach (var l in ledgers)
        {
            running += (l.Debit - l.Credit);

            var line = new CustomerLedgerLineVM
            {
                LedgerId = l.Id,
                Date = l.Date,
                Type = l.Type,
                TypeName = GetLedgerTypeName(l.Type),
                RefId = l.RefId,
                Debit = l.Debit,
                Credit = l.Credit,
                RunningBalance = running,
                Notes = l.Notes
            };

            switch (l.Type)
            {
                case CustomerLedgerType.Sale:
                    line.ReferenceController = "Sales";
                    line.ReferenceAction = "Details";
                    line.ReferenceLabel = $"فاتورة بيع #{l.RefId}";
                    break;

                case CustomerLedgerType.Receipt:
                    line.ReferenceController = "CustomerReceipts";
                    line.ReferenceAction = "Details";
                    if (receiptSalesMap.TryGetValue(l.RefId, out var saleIds) && saleIds.Count > 0)
                    {
                        var salesText = string.Join(", ", saleIds.Select(x => $"#{x}"));
                        line.ReferenceLabel = $"سند قبض #{l.RefId} (لفواتير: {salesText})";
                    }
                    else
                    {
                        line.ReferenceLabel = $"سند قبض #{l.RefId}";
                    }
                    break;

                case CustomerLedgerType.SaleReturn:
                    line.ReferenceController = "SalesReturns";
                    line.ReferenceAction = "Details";
                    if (saleReturnMap.TryGetValue(l.RefId, out var originalSaleId))
                    {
                        line.ReferenceLabel = $"مرتجع مبيعات #{l.RefId} (لفاتورة #{originalSaleId})";
                    }
                    else
                    {
                        line.ReferenceLabel = $"مرتجع مبيعات #{l.RefId}";
                    }
                    break;
            }

            lines.Add(line);
        }

        var vm = new CustomerAccountDetailsVM
        {
            CustomerId = customer.Id,
            CustomerName = customer.Name,
            Phone = customer.Phone,
            SalesmanName = customer.Salesman?.NameAr,
            TotalDebit = ledgers.Sum(x => x.Debit),
            TotalCredit = ledgers.Sum(x => x.Credit),
            Lines = lines
        };

        return View(vm);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.SalesmanId = new SelectList(
            await _context.Salesmen.Where(x => x.IsActive).OrderBy(x => x.NameAr).ToListAsync(),
            "Id",
            "NameAr"
        );
        return View(new Customer());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Customer customer)
    {
        if (customer.SalesmanId == null || customer.SalesmanId <= 0)
            ModelState.AddModelError(nameof(customer.SalesmanId), "اختر المندوب المسؤول عن العميل.");

        if (!ModelState.IsValid)
        {
            ViewBag.SalesmanId = new SelectList(
                await _context.Salesmen.Where(x => x.IsActive).OrderBy(x => x.NameAr).ToListAsync(),
                "Id",
                "NameAr",
                customer.SalesmanId
            );
            return View(customer);
        }

        customer.Name = customer.Name.Trim();
        customer.Phone = customer.Phone?.Trim();
        customer.Address = customer.Address?.Trim();
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "تمت العملية بنجاح";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var customer = await _context.Customers.FirstOrDefaultAsync(x => x.Id == id && x.IsActive);
        if (customer == null) return NotFound();

        ViewBag.SalesmanId = new SelectList(
            await _context.Salesmen.Where(x => x.IsActive).OrderBy(x => x.NameAr).ToListAsync(),
            "Id",
            "NameAr",
            customer.SalesmanId
        );

        return View(customer);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Customer customer)
    {
        if (customer.SalesmanId == null || customer.SalesmanId <= 0)
            ModelState.AddModelError(nameof(customer.SalesmanId), "اختر المندوب المسؤول عن العميل.");

        if (!ModelState.IsValid)
        {
            ViewBag.SalesmanId = new SelectList(
                await _context.Salesmen.Where(x => x.IsActive).OrderBy(x => x.NameAr).ToListAsync(),
                "Id",
                "NameAr",
                customer.SalesmanId
            );
            return View(customer);
        }

        var db = await _context.Customers.FindAsync(customer.Id);
        if (db == null || !db.IsActive) return NotFound();

        db.Name = customer.Name.Trim();
        db.Phone = customer.Phone?.Trim();
        db.Address = customer.Address?.Trim();
        db.SalesmanId = customer.SalesmanId;
        db.IsActive = customer.IsActive;

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "تمت العملية بنجاح";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Disable(int? id)
    {
        if (id == null) return RedirectToAction(nameof(Index));

        var customer = await _context.Customers
            .AsNoTracking()
            .Include(x => x.Salesman)
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);

        if (customer == null) return RedirectToAction(nameof(Index));
        return View(customer);
    }

    [HttpPost, ActionName("Disable")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DisableConfirmed(int? id)
    {
        if (id == null) return RedirectToAction(nameof(Index));

        var customer = await _context.Customers.FindAsync(id);
        if (customer != null)
        {
            customer.IsActive = false;
            await _context.SaveChangesAsync();
        }

        TempData["SuccessMessage"] = "تمت العملية بنجاح";
        return RedirectToAction(nameof(Index));
    }

    private static string GetLedgerTypeName(CustomerLedgerType type) => type switch
    {
        CustomerLedgerType.Sale => "فاتورة بيع",
        CustomerLedgerType.Receipt => "سند قبض",
        CustomerLedgerType.SaleReturn => "مرتجع مبيعات",
        _ => "عملية"
    };
}
