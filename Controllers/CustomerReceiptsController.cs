using AbuAmenPharma.Data;
using AbuAmenPharma.Models;
using AbuAmenPharma.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Admin,Operator")]
public class CustomerReceiptsController : Controller
{
    private readonly ApplicationDbContext _context;
    public CustomerReceiptsController(ApplicationDbContext context) => _context = context;

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
        term = term?.Trim();
        var q = _context.Customers.AsNoTracking().Where(x => x.IsActive);

        if (!string.IsNullOrEmpty(term))
            q = q.Where(x => x.Name.Contains(term));

        var data = await q.OrderBy(x => x.Name)
            .Take(30)
            .Select(x => new { id = x.Id, text = x.Name })
            .ToListAsync();

        return Json(data);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.CustomerId = new SelectList(
            await _context.Customers.Where(x => x.IsActive).OrderBy(x => x.Name).ToListAsync(),
            "Id", "Name"
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
            return View(vm);
        }


        using var trx = await _context.Database.BeginTransactionAsync();

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

        // 2) تحديد الفواتير الهدف
        List<Sale> targetSales;

        if (!vm.AutoAllocate)
        {
            var sale = await _context.Sales.FirstOrDefaultAsync(s =>
                s.Id == vm.SaleId && s.CustomerId == vm.CustomerId && s.RemainingAmount > 0);

            if (sale == null)
            {
                ModelState.AddModelError("", "الفاتورة غير موجودة أو ليس عليها متبقي.");
                await trx.RollbackAsync();

                ViewBag.CustomerId = new SelectList(
                    await _context.Customers.Where(x => x.IsActive).OrderBy(x => x.Name).ToListAsync(),
                    "Id", "Name", vm.CustomerId
                );
                return View(vm);
            }

            targetSales = new List<Sale> { sale };
        }
        else
        {
            // توزيع تلقائي على أقدم فواتير عليها متبقي
            targetSales = await _context.Sales
                .Where(s => s.CustomerId == vm.CustomerId && s.IsPosted && s.RemainingAmount > 0)
                .OrderBy(s => s.SaleDate)
                .ThenBy(s => s.Id)
                .ToListAsync();
        }

        // 3) إنشاء Allocations وتحديث Sale Paid/Remaining
        foreach (var sale in targetSales)
        {
            if (remainingToAllocate <= 0) break;

            var canPay = sale.RemainingAmount;
            if (canPay <= 0) continue;

            var pay = Math.Min(canPay, remainingToAllocate);

            _context.CustomerReceiptAllocations.Add(new CustomerReceiptAllocation
            {
                ReceiptId = receipt.Id,
                SaleId = sale.Id,
                Amount = pay
            });

            sale.PaidAmount += pay;
            sale.RemainingAmount = sale.NetTotal - sale.PaidAmount;

            remainingToAllocate -= pay;
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
                ? $"سند قبض رقم {receipt.Id} (رصيد دائن غير موزع: {remainingToAllocate})"
                : $"سند قبض رقم {receipt.Id}"
        });

        await _context.SaveChangesAsync();
        await trx.CommitAsync();

        return RedirectToAction(nameof(Index));
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

        // 1) جلب السند
        var receipt = await _context.CustomerReceipts
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);

        if (receipt == null) return NotFound();

        // 2) جلب التخصيصات النشطة
        var allocs = await _context.CustomerReceiptAllocations
            .Where(a => a.ReceiptId == id && a.IsActive)
            .ToListAsync();

        // 3) رجوع المبالغ من الفواتير
        if (allocs.Count > 0)
        {
            var saleIds = allocs.Select(a => a.SaleId).Distinct().ToList();

            var sales = await _context.Sales
                .Where(s => saleIds.Contains(s.Id))
                .ToListAsync();

            foreach (var a in allocs)
            {
                var sale = sales.FirstOrDefault(s => s.Id == a.SaleId);
                if (sale == null) continue;

                sale.PaidAmount -= a.Amount;
                if (sale.PaidAmount < 0) sale.PaidAmount = 0; // حماية
                sale.RemainingAmount = sale.NetTotal - sale.PaidAmount;

                a.IsActive = false; // تعطيل التخصيص
            }
        }

        // 4) تعطيل قيد الدفتر المرتبط بالسند
        var ledgers = await _context.CustomerLedgers
            .Where(l => l.Type == CustomerLedgerType.Receipt && l.RefId == receipt.Id && l.IsActive)
            .ToListAsync();

        foreach (var l in ledgers)
            l.IsActive = false;

        // 5) تعطيل السند نفسه
        receipt.IsActive = false;

        await _context.SaveChangesAsync();
        await trx.CommitAsync();

        return RedirectToAction(nameof(Index));
    }
}