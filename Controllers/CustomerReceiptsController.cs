using AbuAmenPharma.Data;
using AbuAmenPharma.Helpers;
using AbuAmenPharma.Models;
using AbuAmenPharma.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AbuAmenPharma.Services;

[Authorize(Roles = "Admin,Operator")]
public class CustomerReceiptsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IFinancialService _financialService;

    public CustomerReceiptsController(ApplicationDbContext context, IFinancialService financialService)
    {
        _context = context;
        _financialService = financialService;
    }

    public async Task<IActionResult> Index()
    {
        var data = await _context.CustomerReceipts
            .AsNoTracking()
            .Where(x => x.IsActive)
            .Include(x => x.Customer)
            .OrderByDescending(x => x.Id)
            .Take(300)
            .ToListAsync();

        return View(data);
    }

    public async Task<IActionResult> Details(long id)
    {
        var receipt = await _context.CustomerReceipts
            .AsNoTracking()
            .Include(r => r.Customer)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (receipt == null) return NotFound();

        var allocs = await _context.CustomerReceiptAllocations
            .AsNoTracking()
            .Where(a => a.ReceiptId == id && a.IsActive)
            .Include(a => a.Sale)
            .OrderBy(a => a.Sale.SaleDate)
            .ThenBy(a => a.SaleId)
            .ToListAsync();

        ViewBag.Allocations = allocs;
        return View(receipt);
    }

    [HttpGet]
    public async Task<IActionResult> SearchCustomers(string term)
    {
        term = (term ?? string.Empty).Trim();
        var q = _context.Customers.AsNoTracking().Where(x => x.IsActive);

        if (!string.IsNullOrEmpty(term))
            q = q.Where(x => x.Name.Contains(term));

        var data = await q.OrderBy(x => x.Name)
            .Take(30)
            .Select(x => new { id = x.Id, text = x.Name })
            .ToListAsync();

        return Json(data);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCustomerAjax(string name, int salesmanId)
    {
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest("اسم العميل مطلوب");

        var salesman = await _context.Salesmen.FirstOrDefaultAsync(x => x.Id == salesmanId && x.IsActive);
        if (salesman == null)
            return BadRequest("اختيار المندوب المسؤول عن العميل مطلوب");

        var normalizedName = NameNormalizer.NormalizeForLookup(name);
        var existing = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.IsActive && x.NameNormalized == normalizedName);

        if (existing != null)
            return Conflict(new { message = "هذا العميل موجود مسبقاً.", id = existing.Id, text = existing.Name });

        var customer = new Customer
        {
            Name = name.Trim(),
            NameNormalized = normalizedName,
            SalesmanId = salesman.Id,
            IsActive = true
        };
        _context.Customers.Add(customer);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex, "IX_Customers_NameNormalized_Active"))
        {
            existing = await _context.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IsActive && x.NameNormalized == normalizedName);

            return Conflict(new
            {
                message = "هذا العميل موجود مسبقاً.",
                id = existing?.Id,
                text = existing?.Name ?? name.Trim()
            });
        }

        return Json(new { id = customer.Id, text = customer.Name });
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.CustomerId = new SelectList(
            await _context.Customers.Where(x => x.IsActive).OrderBy(x => x.Name).ToListAsync(),
            "Id", "Name"
        );
        ViewBag.SalesmanId = new SelectList(
            await _context.Salesmen.Where(x => x.IsActive).OrderBy(x => x.NameAr).ToListAsync(),
            "Id",
            "NameAr"
        );
        return View(new CustomerReceiptCreateVM());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CustomerReceiptCreateVM vm)
    {
        if (vm.Amount <= 0)
            ModelState.AddModelError(nameof(vm.Amount), "المبلغ يجب أن يكون أكبر من صفر.");

        if (!vm.AutoAllocate && (vm.SaleId == null || vm.SaleId <= 0))
            ModelState.AddModelError(nameof(vm.SaleId), "اختر فاتورة عند إلغاء التوزيع التلقائي.");

        if (!ModelState.IsValid)
        {
            ViewBag.CustomerId = new SelectList(
                await _context.Customers.Where(x => x.IsActive).OrderBy(x => x.Name).ToListAsync(),
                "Id", "Name", vm.CustomerId
            );
            ViewBag.SalesmanId = new SelectList(
                await _context.Salesmen.Where(x => x.IsActive).OrderBy(x => x.NameAr).ToListAsync(),
                "Id",
                "NameAr"
            );
            return View(vm);
        }

        using var trx = await _context.Database.BeginTransactionAsync();

        try
        {
            // 1) إنشاء السند
            var receipt = new CustomerReceipt
            {
                Date = vm.Date,
                CustomerId = vm.CustomerId,
                Amount = vm.Amount,
                Notes = vm.Notes?.Trim(),
                IsActive = true
            };

            _context.CustomerReceipts.Add(receipt);
            await _context.SaveChangesAsync();

            decimal remainingToAllocate = vm.Amount;

            if (vm.AutoAllocate)
            {
                // Use Centralized Service for FIFO Allocation
                remainingToAllocate = await _financialService.AllocatePaymentAsync(receipt.Id, vm.CustomerId, vm.Amount);
            }
            else
            {
                // Manual Allocation for a specific invoice
                var sale = await _context.Sales.FirstOrDefaultAsync(s =>
                    s.Id == vm.SaleId && s.CustomerId == vm.CustomerId && s.RemainingAmount > 0);

                if (sale == null)
                {
                    ModelState.AddModelError("", "الفاتورة غير موجودة أو ليس عليها متبقي.");
                    await trx.RollbackAsync();
                    // ... re-populating viewbags omitted for brevity or handled by returning view
                    return RedirectToAction(nameof(Create)); // Simplified for this example
                }

                decimal pay = Math.Min(sale.RemainingAmount, vm.Amount);
                _context.CustomerReceiptAllocations.Add(new CustomerReceiptAllocation
                {
                    ReceiptId = receipt.Id,
                    SaleId = sale.Id,
                    Amount = pay
                });

                sale.PaidAmount += pay;
                sale.RemainingAmount = Math.Max(0, sale.NetTotal - sale.PaidAmount);
                remainingToAllocate = Math.Max(0, vm.Amount - pay);
            }

            // ✅ الباقي يصبح رصيد دائن غير موزع
            receipt.UnallocatedAmount = remainingToAllocate;

            // 4) قيد دفتر العميل (Credit كامل مبلغ السند)
            _context.CustomerLedgers.Add(new CustomerLedger
            {
                Date = receipt.Date,
                CustomerId = receipt.CustomerId,
                Type = CustomerLedgerType.Receipt,
                RefId = receipt.Id,
                Debit = 0,
                Credit = receipt.Amount,
                Notes = remainingToAllocate > 0
                    ? $"سند قبض رقم {receipt.Id} (رصيد دائن غير موزع: {remainingToAllocate:N2})"
                    : $"سند قبض رقم {receipt.Id}"
            });

            await _context.SaveChangesAsync();
            await trx.CommitAsync();

            TempData["SuccessMessage"] = "تم حفظ سند القبض بنجاح!";

            return RedirectToAction(nameof(Index));
        }
        catch (Exception)
        {
            await trx.RollbackAsync();
            ModelState.AddModelError("", "حدث خطأ أثناء حفظ السند. يرجى المحاولة مرة أخرى.");
            ViewBag.CustomerId = new SelectList(
                await _context.Customers.Where(x => x.IsActive).OrderBy(x => x.Name).ToListAsync(),
                "Id", "Name", vm.CustomerId
            );
            ViewBag.SalesmanId = new SelectList(
                await _context.Salesmen.Where(x => x.IsActive).OrderBy(x => x.NameAr).ToListAsync(),
                "Id",
                "NameAr"
            );
            return View(vm);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetOpenInvoices(int customerId)
    {
        var invoices = await _context.Sales
            .AsNoTracking()
            .Where(s => s.CustomerId == customerId && s.IsPosted && s.RemainingAmount > 0)
            .OrderBy(s => s.SaleDate)
            .Select(s => new
            {
                id = s.Id,
                text = $"فاتورة #{s.Id} - بتاريخ {s.SaleDate:yyyy-MM-dd} - المتبقي {s.RemainingAmount}"
            })
            .ToListAsync();

        return Json(invoices);
    }

    [HttpGet]
    public async Task<IActionResult> GetCustomerBalance(int customerId)
    {
        var balance = await _financialService.GetCustomerDueBalanceAsync(customerId);
        return Json(new { balance });
    }

    public async Task<IActionResult> Disable(long id)
    {
        var rec = await _context.CustomerReceipts
            .Include(x => x.Customer)
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);

        if (rec == null) return NotFound();
        return View(rec);
    }

    [HttpPost, ActionName("Disable")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DisableConfirmed(long id)
    {
        using var trx = await _context.Database.BeginTransactionAsync();

        try
        {
            // 1) جلب السند
            var receipt = await _context.CustomerReceipts
                .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);

            if (receipt == null) return NotFound();

            // 2) Reverse Allocations using Centralized Service
            await _financialService.ReversePaymentAllocationAsync(id);

            // 3) Mark receipt allocations as inactive for history (optional, service handles removal or deactivation)
            var allocsToDeactivate = await _context.CustomerReceiptAllocations
                .Where(a => a.ReceiptId == id && a.IsActive)
                .ToListAsync();
            foreach (var a in allocsToDeactivate) a.IsActive = false;

            // 4) تعطيل قيد الدفتر المرتبط بالسند
            var ledgers = await _context.CustomerLedgers
                .Where(l => l.Type == CustomerLedgerType.Receipt && l.RefId == receipt.Id && l.IsActive)
                .ToListAsync();

            foreach (var l in ledgers)
                l.IsActive = false;

            // 5) إنشاء قيد عكسي في الدفتر (Debit لإلغاء الأثر)
            _context.CustomerLedgers.Add(new CustomerLedger
            {
                Date = DateTime.Now,
                CustomerId = receipt.CustomerId,
                Type = CustomerLedgerType.Receipt,
                RefId = receipt.Id,
                Debit = receipt.Amount,
                Credit = 0,
                Notes = $"إلغاء سند قبض رقم {receipt.Id}"
            });

            // 6) تعطيل السند نفسه
            receipt.IsActive = false;

            await _context.SaveChangesAsync();
            await trx.CommitAsync();

            TempData["SuccessMessage"] = "تم تعطيل السند بنجاح وإرجاع المبالغ إلى الفواتير.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception)
        {
            await trx.RollbackAsync();
            TempData["ErrorMessage"] = "حدث خطأ أثناء تعطيل السند. يرجى المحاولة مرة أخرى.";
            return RedirectToAction(nameof(Index));
        }
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex, string indexName)
        => ex.InnerException?.Message.Contains(indexName, StringComparison.OrdinalIgnoreCase) == true;
}
