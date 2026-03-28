using AbuAmenPharma.Data;
using AbuAmenPharma.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Admin,Operator")]
public class ItemsController : Controller
{
    private readonly ApplicationDbContext _context;
    public ItemsController(ApplicationDbContext context) => _context = context;

    public IActionResult Index() => View();

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var item = await _context.Items
            .Include(x => x.Unit)
            .Include(x => x.Manufacturer)
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);

        if (item == null) return NotFound();
        return View(item);
    }

    private void FillLookups(Item? item = null)
    {
        ViewData["UnitId"] = new SelectList(_context.Units.Where(x => x.IsActive), "Id", "NameAr", item?.UnitId);
        ViewData["ManufacturerId"] = new SelectList(_context.Manufacturers.Where(x => x.IsActive), "Id", "NameAr", item?.ManufacturerId);
    }

    public IActionResult Create()
    {
        FillLookups();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Item item)
    {
        if (!ModelState.IsValid)
        {
            FillLookups(item);
            return View(item);
        }

        item.NameAr = item.NameAr.Trim();
        item.BarCode = item.BarCode?.Trim();

        _context.Add(item);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var item = await _context.Items.FirstOrDefaultAsync(x => x.Id == id && x.IsActive);
        if (item == null) return NotFound();

        FillLookups(item);
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Item item)
    {
        if (id != item.Id) return NotFound();

        if (!ModelState.IsValid)
        {
            FillLookups(item);
            return View(item);
        }

        var db = await _context.Items.FindAsync(id);
        if (db == null || !db.IsActive) return NotFound();

        db.NameAr = item.NameAr.Trim();
        db.BarCode = item.BarCode?.Trim();
        db.UnitId = item.UnitId;
        db.ManufacturerId = item.ManufacturerId;
        db.DefaultPurchasePrice = item.DefaultPurchasePrice;
        db.DefaultSellPrice = item.DefaultSellPrice;
        db.ReorderLevel = item.ReorderLevel;

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Disable(int? id)
    {
        if (id == null) return NotFound();
        var item = await _context.Items
            .Include(x => x.Unit)
            .Include(x => x.Manufacturer)
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);

        if (item == null) return NotFound();
        return View(item);
    }

    [HttpPost, ActionName("Disable")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DisableConfirmed(int id)
    {
        var item = await _context.Items.FindAsync(id);
        if (item != null)
        {
            item.IsActive = false;
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> LoadData()
    {
        var draw = Request.Form["draw"].FirstOrDefault();
        var start = Convert.ToInt32(Request.Form["start"].FirstOrDefault() ?? "0");
        var length = Convert.ToInt32(Request.Form["length"].FirstOrDefault() ?? "10");
        var searchValue = Request.Form["search[value]"].FirstOrDefault()?.Trim();

        var sortColumnIndex = Request.Form["order[0][column]"].FirstOrDefault();
        var sortColumnName = Request.Form[$"columns[{sortColumnIndex}][data]"].FirstOrDefault();
        var sortDirection = Request.Form["order[0][dir]"].FirstOrDefault(); // asc/desc

        // Query أساس
        var query = _context.Items
            .AsNoTracking()
            .Where(x => x.IsActive)
            .Include(x => x.Unit)
            .Include(x => x.Manufacturer)
            .Select(x => new
            {
                x.Id,
                x.NameAr,
                BarCode = x.BarCode,
                UnitName = x.Unit!.NameAr,
                ManufacturerName = x.Manufacturer!.NameAr,
                x.DefaultSellPrice
            });

        var recordsTotal = await query.CountAsync();

        // Search
        if (!string.IsNullOrWhiteSpace(searchValue))
        {
            query = query.Where(x =>
                x.NameAr.Contains(searchValue) ||
                (x.BarCode != null && x.BarCode.Contains(searchValue)) ||
                x.UnitName.Contains(searchValue) ||
                x.ManufacturerName.Contains(searchValue)
            );
        }

        var recordsFiltered = await query.CountAsync();

        // Sorting (بشكل بسيط وواضح)
        query = (sortColumnName, sortDirection) switch
        {
            ("NameAr", "desc") => query.OrderByDescending(x => x.NameAr),
            ("NameAr", _) => query.OrderBy(x => x.NameAr),

            ("BarCode", "desc") => query.OrderByDescending(x => x.BarCode),
            ("BarCode", _) => query.OrderBy(x => x.BarCode),

            ("UnitName", "desc") => query.OrderByDescending(x => x.UnitName),
            ("UnitName", _) => query.OrderBy(x => x.UnitName),

            ("ManufacturerName", "desc") => query.OrderByDescending(x => x.ManufacturerName),
            ("ManufacturerName", _) => query.OrderBy(x => x.ManufacturerName),

            ("DefaultSellPrice", "desc") => query.OrderByDescending(x => x.DefaultSellPrice),
            ("DefaultSellPrice", _) => query.OrderBy(x => x.DefaultSellPrice),

            _ => query.OrderBy(x => x.Id)
        };

        // Paging
        var data = await query.Skip(start).Take(length).ToListAsync();

        return Json(new
        {
            draw,
            recordsTotal,
            recordsFiltered,
            data
        });
    }
}