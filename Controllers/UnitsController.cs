using AbuAmenPharma.Data;
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
        if (!ModelState.IsValid) return View(unit);

        unit.NameAr = unit.NameAr.Trim();
        _context.Add(unit);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "تمت العملية بنجاح";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> CreateAjax(string nameAr)
    {
        if (string.IsNullOrWhiteSpace(nameAr)) return BadRequest("الاسم مطلوب");
        var obj = new Unit { NameAr = nameAr.Trim(), IsActive = true };
        _context.Add(obj);
        await _context.SaveChangesAsync();
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
        if (!ModelState.IsValid) return View(unit);

        var db = await _context.Units.FindAsync(unit.Id);
        if (db == null || !db.IsActive) return NotFound();

        db.NameAr = unit.NameAr.Trim();
        db.NameEn = unit.NameEn?.Trim();
        await _context.SaveChangesAsync();
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
}
