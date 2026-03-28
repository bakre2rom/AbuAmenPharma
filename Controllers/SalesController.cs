using AbuAmenPharma.Data;
using AbuAmenPharma.Models;
using AbuAmenPharma.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Admin,Operator")]
public class SalesController : Controller
{
    private readonly ApplicationDbContext _context;
    public SalesController(ApplicationDbContext context) => _context = context;

    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> Print(long id)
    {
        var sale = await _context.Sales
            .AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.Salesman)
            .Include(x => x.Lines).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (sale == null) return NotFound();

        var allocations = await _context.SaleAllocations
            .AsNoTracking()
            .Where(a => a.SaleLine!.SaleId == id)
            .Include(a => a.Batch)
            .ToListAsync();

        ViewBag.Allocations = allocations;
        return View(sale);
    }

    public async Task<IActionResult> Details(long id)
    {
        var sale = await _context.Sales
            .AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.Salesman)
            .Include(x => x.Lines)
                .ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (sale == null) return NotFound();

        // allocations + batch info
        var allocations = await _context.SaleAllocations
            .AsNoTracking()
            .Where(a => a.SaleLine!.SaleId == id)
            .Include(a => a.Batch)
            .ToListAsync();

        ViewBag.Allocations = allocations;
        return View(sale);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.CustomerId = new SelectList(await _context.Customers.Where(x => x.IsActive).OrderBy(x => x.Name).ToListAsync(), "Id", "Name");
        ViewBag.SalesmanId = new SelectList(await _context.Salesmen.Where(x => x.IsActive).OrderBy(x => x.NameAr).ToListAsync(), "Id", "NameAr");
        return View(new SaleCreateVM());
    }

    // Select2 Search Items
    [HttpGet]
    public async Task<IActionResult> SearchItems(string term)
    {
        term = (term ?? "").Trim();
        var q = _context.Items.AsNoTracking().Where(x => x.IsActive);

        if (!string.IsNullOrWhiteSpace(term))
            q = q.Where(x => x.NameAr.Contains(term) || (x.BarCode != null && x.BarCode.Contains(term)));

        var items = await q.OrderBy(x => x.NameAr)
            .Take(20)
            .Select(x => new { id = x.Id, text = x.NameAr + (x.BarCode != null ? $" - {x.BarCode}" : "") })
            .ToListAsync();

        return Json(items);
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

    [HttpGet]
    public async Task<IActionResult> GetItemAvailability(int id)
    {
        // دفعات الصنف FIFO
        var batches = await _context.ItemBatches
            .AsNoTracking()
            .Where(b => b.ItemId == id && b.IsActive)
            .Select(b => new
            {
                b.Id,
                b.BatchNo,
                b.ExpiryDate
            })
            .ToListAsync();

        var balances = await _context.StockMovements
            .AsNoTracking()
            .Where(m => m.ItemId == id)
            .GroupBy(m => m.BatchId)
            .Select(g => new { BatchId = g.Key, Balance = g.Sum(x => x.QtyIn - x.QtyOut) })
            .ToListAsync();

        var balanceDict = balances.ToDictionary(x => x.BatchId, x => x.Balance);

        var result = batches
            .Select(b => new
            {
                batchId = b.Id,
                batchNo = b.BatchNo,
                expiryDate = b.ExpiryDate, // nullable
                balance = balanceDict.TryGetValue(b.Id, out var bal) ? bal : 0
            })
            .Where(x => x.balance > 0)
            .OrderBy(x => x.expiryDate)   // null في الآخر
            .ThenBy(x => x.expiryDate)
            .ThenBy(x => x.batchId)
            .ToList();

        var totalAvailable = result.Sum(x => x.balance);

        return Json(new
        {
            totalAvailable,
            batches = result.Select(x => new
            {
                x.batchNo,
                expiry = x.expiryDate.ToString("yyyy-MM-dd") ?? "-",
                x.balance
            }).ToList()
        });
    }

    // (اختياري) API لجلب سعر البيع للصنف عند الاختيار
    [HttpGet]
    public async Task<IActionResult> GetItemPrice(int id)
    {
        var price = await _context.Items.Where(x => x.Id == id).Select(x => x.DefaultSellPrice).FirstOrDefaultAsync();
        return Json(new { price });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SaleCreateVM vm)
    {
        if (vm.Lines == null || vm.Lines.Count == 0)
            ModelState.AddModelError("", "أضف بند واحد على الأقل.");

        if (!ModelState.IsValid)
            return View(vm);

        using var trx = await _context.Database.BeginTransactionAsync();

        var sale = new Sale
        {
            SaleDate = vm.SaleDate,
            CustomerId = vm.CustomerId,
            SalesmanId = vm.SalesmanId,
            PaymentMode = vm.PaymentMode,
            Discount = vm.Discount,
            PaidAmount = vm.PaidAmount,
            Notes = vm.Notes?.Trim()
        };

        // تجهيز خطوط البيع (UnitPrice ثابت من Item)
        foreach (var l in vm.Lines.Where(x => x.ItemId > 0 && x.Qty > 0))
        {
            if (l.UnitPrice < 0)
            {
                ModelState.AddModelError("", "سعر البيع لا يمكن أن يكون سالب.");
                await trx.RollbackAsync();
                return View(vm);
            }

            sale.Lines.Add(new SaleLine
            {
                ItemId = l.ItemId,
                Qty = l.Qty,
                UnitPrice = l.UnitPrice,
                LineTotal = l.Qty * l.UnitPrice
            });
        }

        if (sale.Lines.Count == 0)
        {
            ModelState.AddModelError("", "بنود الفاتورة غير صحيحة.");
            await trx.RollbackAsync();
            return View(vm);
        }

        sale.SubTotal = sale.Lines.Sum(x => x.LineTotal);
        sale.NetTotal = sale.SubTotal - sale.Discount;
        sale.RemainingAmount = sale.NetTotal - sale.PaidAmount;

        // قواعد الدفع
        if (sale.PaidAmount < 0 || sale.PaidAmount > sale.NetTotal)
        {
            ModelState.AddModelError("", "قيمة المدفوع غير صحيحة.");
            await trx.RollbackAsync();
            return View(vm);
        }

        if (sale.PaymentMode == SalePaymentMode.Cash && sale.RemainingAmount != 0)
        {
            ModelState.AddModelError("", "في البيع النقدي يجب أن يكون المتبقي = 0.");
            await trx.RollbackAsync();
            return View(vm);
        }

        // ✅ تحقق التوفر + عمل FIFO allocations + StockMovements
        _context.Sales.Add(sale);
        await _context.SaveChangesAsync(); // للحصول على sale.Id

        foreach (var line in sale.Lines)
        {
            var qtyNeeded = line.Qty;

            var batches = await GetFifoBatchesAsync(line.ItemId);

            foreach (var b in batches)
            {
                if (qtyNeeded <= 0) break;

                var balance = await GetBatchBalanceAsync(b.Id);
                if (balance <= 0) continue;

                var take = Math.Min(balance, qtyNeeded);

                // Allocation
                var alloc = new SaleAllocation
                {
                    SaleLineId = line.Id,
                    BatchId = b.Id,
                    Qty = take
                };
                _context.SaleAllocations.Add(alloc);

                // StockMovement QtyOut
                _context.StockMovements.Add(new StockMovement
                {
                    Date = sale.SaleDate,
                    ItemId = line.ItemId,
                    BatchId = b.Id,
                    QtyIn = 0,
                    QtyOut = take,
                    UnitCost = b.PurchasePrice, // للتكلفة (للأرباح لاحقاً)
                    RefType = StockRefType.Sale,
                    RefId = sale.Id,
                    Notes = $"فاتورة بيع رقم: {sale.Id}"
                });

                qtyNeeded -= take;
            }

            // إذا لم نغطي الكمية المطلوبة بالكامل → منع البيع
            if (qtyNeeded > 0)
            {
                ModelState.AddModelError("", $"لا توجد كمية كافية في المخزون للصنف رقم ({line.ItemId}). المتبقي غير متاح: {qtyNeeded}");
                await trx.RollbackAsync();
                return View(vm);
            }
        }

        // ✅ Customer Ledger
        // قيد الفاتورة (Debit على العميل إذا فيها آجِل أو حتى نقدي للتوثيق)
        _context.CustomerLedgers.Add(new CustomerLedger
        {
            Date = sale.SaleDate,
            CustomerId = sale.CustomerId,
            Type = CustomerLedgerType.Sale,
            RefId = sale.Id,
            Debit = sale.NetTotal,
            Credit = 0,
            Notes = $"فاتورة بيع رقم {sale.Id}"
        });

        // قيد التحصيل داخل الفاتورة (إن وجد)
        if (sale.PaidAmount > 0)
        {
            _context.CustomerLedgers.Add(new CustomerLedger
            {
                Date = sale.SaleDate,
                CustomerId = sale.CustomerId,
                Type = CustomerLedgerType.Receipt,
                RefId = sale.Id,
                Debit = 0,
                Credit = sale.PaidAmount,
                Notes = $"تحصيل داخل فاتورة بيع رقم {sale.Id}"
            });
        }

        sale.IsPosted = true;
        await _context.SaveChangesAsync();
        await trx.CommitAsync();

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> GetSalesData()
    {
        try
        {
            // قراءة بيانات DataTable من الـ Request
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault()?.ToLower() ?? "";

            int pageSize = length != null ? int.Parse(length) : 25;
            int skip = start != null ? int.Parse(start) : 0;

            // الاستعلام الأساسي
            var query = _context.Sales
                .AsNoTracking()
                .Include(x => x.Customer)
                .Include(x => x.Salesman)
                .AsQueryable();

            // العدد الإجمالي
            int totalRecords = await query.CountAsync();

            // تطبيق البحث
            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(x =>
                    x.Id.ToString().Contains(searchValue) ||
                    x.Customer.Name.Contains(searchValue) ||
                    (x.Salesman != null && x.Salesman.NameAr.Contains(searchValue)) ||
                    x.NetTotal.ToString().Contains(searchValue)
                );
            }

            int filteredRecords = await query.CountAsync();

            // جلب البيانات مع التقسيم
            var data = await query
                .OrderByDescending(x => x.Id)
                .Skip(skip)
                .Take(pageSize)
                .Select(x => new SaleRow
                {
                    id = (int)x.Id,
                    saleDate = x.SaleDate.ToString("yyyy-MM-dd"),
                    customerName = x.Customer.Name,
                    salesmanName = x.Salesman != null ? x.Salesman.NameAr : "-",
                    paymentMode = x.PaymentMode.ToString(),
                    netTotal = x.NetTotal,
                    paidAmount = x.PaidAmount,
                    remainingAmount = x.RemainingAmount
                })
                .ToListAsync();

            // تجهيز الاستجابة
            var response = new DataTableResponse
            {
                draw = int.Parse(draw),
                recordsTotal = totalRecords,
                recordsFiltered = filteredRecords,
                data = data
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return Ok(new DataTableResponse
            {
                draw = int.Parse(Request.Form["draw"].FirstOrDefault() ?? "1"),
                recordsTotal = 0,
                recordsFiltered = 0,
                data = new List<SaleRow>()
            });
        }
    }

    private async Task<decimal> GetBatchBalanceAsync(int batchId)
        => await _context.StockMovements.Where(m => m.BatchId == batchId).Select(m => m.QtyIn - m.QtyOut).SumAsync();

    private async Task<List<ItemBatch>> GetFifoBatchesAsync(int itemId)
        => await _context.ItemBatches.Where(b => b.ItemId == itemId && b.IsActive)
            .OrderBy(b => b.ExpiryDate == null)
            .ThenBy(b => b.ExpiryDate)
            .ThenBy(b => b.Id)
            .ToListAsync();
}