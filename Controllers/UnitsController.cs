using AbuAmenPharma.Data;
using AbuAmenPharma.Helpers;
using AbuAmenPharma.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Admin")]
public class UnitsController : Controller
{
    private readonly ApplicationDbContext _context;
    public UnitsController(ApplicationDbContext context) => _context = context;

    // عرض الوحدات النشطة فقط
    public async Task<IActionResult> Index()
        => View(await _context.Units.Where(x => x.IsActive).OrderBy(x => x.NameAr).ToListAsync());

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var unit = await _context.Units.FirstOrDefaultAsync(x => x.Id == id && x.IsActive);
        if (unit == null) return NotFound();
        return View(unit);
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Unit unit)
    {
        if (string.IsNullOrWhiteSpace(unit.NameAr))
            ModelState.AddModelError(nameof(unit.NameAr), "حقل الاسم مطلوب");

        if (!ModelState.IsValid) return View(unit);

        unit.NameAr = unit.NameAr.Trim();
        unit.NameArNormalized = NameNormalizer.NormalizeForLookup(unit.NameAr);

        var exists = await _context.Units.AnyAsync(x =>
            x.IsActive && x.NameArNormalized == unit.NameArNormalized);
        if (exists)
        {
            ModelState.AddModelError(nameof(unit.NameAr), "هذه الوحدة موجودة مسبقاً.");
            return View(unit);
        }

        _context.Add(unit);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex, "IX_Units_NameArNormalized_Active"))
        {
            ModelState.AddModelError(nameof(unit.NameAr), "هذه الوحدة موجودة مسبقاً.");
            return View(unit);
        }

        TempData["SuccessMessage"] = "تمت العملية بنجاح";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> CreateAjax(string nameAr)
    {
        if (string.IsNullOrWhiteSpace(nameAr))
            return BadRequest(new { message = "الاسم مطلوب" });

        var trimmedName = nameAr.Trim();
        var normalizedName = NameNormalizer.NormalizeForLookup(trimmedName);

        var existing = await _context.Units
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.IsActive && x.NameArNormalized == normalizedName);

        if (existing != null)
        {
            return Conflict(new
            {
                message = "هذه الوحدة موجودة مسبقاً.",
                id = existing.Id,
                name = existing.NameAr
            });
        }

        var obj = new Unit
        {
            NameAr = trimmedName,
            NameArNormalized = normalizedName,
            IsActive = true
        };
        _context.Add(obj);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex, "IX_Units_NameArNormalized_Active"))
        {
            existing = await _context.Units
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IsActive && x.NameArNormalized == normalizedName);

            return Conflict(new
            {
                message = "هذه الوحدة موجودة مسبقاً.",
                id = existing?.Id,
                name = existing?.NameAr ?? trimmedName
            });
        }

        return Json(new { id = obj.Id, name = obj.NameAr });
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var unit = await _context.Units.FirstOrDefaultAsync(x => x.Id == id && x.IsActive);
        if (unit == null) return NotFound();
        return View(unit);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Unit unit)
    {
        if (string.IsNullOrWhiteSpace(unit.NameAr))
            ModelState.AddModelError(nameof(unit.NameAr), "حقل الاسم مطلوب");

        if (!ModelState.IsValid) return View(unit);

        var db = await _context.Units.FindAsync(unit.Id);
        if (db == null || !db.IsActive) return NotFound();

        db.NameAr = unit.NameAr.Trim();
        db.NameArNormalized = NameNormalizer.NormalizeForLookup(db.NameAr);
        db.NameEn = unit.NameEn?.Trim();

        var exists = await _context.Units.AnyAsync(x =>
            x.IsActive &&
            x.Id != db.Id &&
            x.NameArNormalized == db.NameArNormalized);

        if (exists)
        {
            ModelState.AddModelError(nameof(unit.NameAr), "هذه الوحدة موجودة مسبقاً.");
            return View(unit);
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex, "IX_Units_NameArNormalized_Active"))
        {
            ModelState.AddModelError(nameof(unit.NameAr), "هذه الوحدة موجودة مسبقاً.");
            return View(unit);
        }

        TempData["SuccessMessage"] = "تمت العملية بنجاح";
        return RedirectToAction(nameof(Index));
    }

    // شاشة تأكيد التعطيل
    public async Task<IActionResult> Disable(int? id)
    {
        if (id == null) return NotFound();
        var unit = await _context.Units.FirstOrDefaultAsync(x => x.Id == id && x.IsActive);
        if (unit == null) return NotFound();
        return View(unit);
    }

    [HttpPost, ActionName("Disable")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DisableConfirmed(int id)
    {
        var unit = await _context.Units.FindAsync(id);
        if (unit != null)
        {
            unit.IsActive = false;
            await _context.SaveChangesAsync();
        }
        TempData["SuccessMessage"] = "تمت العملية بنجاح";
        return RedirectToAction(nameof(Index));
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex, string indexName)
        => ex.InnerException?.Message.Contains(indexName, StringComparison.OrdinalIgnoreCase) == true;
}
