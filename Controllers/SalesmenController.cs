using AbuAmenPharma.Data;
using AbuAmenPharma.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Admin")]
public class SalesmenController : Controller
{
    private readonly ApplicationDbContext _context;
    public SalesmenController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index()
        => View(await _context.Salesmen.Where(x => x.IsActive).OrderBy(x => x.NameAr).ToListAsync());

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Salesman model)
    {
        if (!ModelState.IsValid) return View(model);

        _context.Salesmen.Add(model);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var s = await _context.Salesmen.FindAsync(id);
        if (s == null || !s.IsActive) return NotFound();
        return View(s);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, Salesman model)
    {
        var db = await _context.Salesmen.FindAsync(id);
        if (db == null) return NotFound();

        db.NameAr = model.NameAr;
        db.Phone = model.Phone;

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Disable(int id)
    {
        var s = await _context.Salesmen.FindAsync(id);
        if (s == null) return NotFound();
        return View(s);
    }

    [HttpPost, ActionName("Disable")]
    public async Task<IActionResult> DisableConfirmed(int id)
    {
        var s = await _context.Salesmen.FindAsync(id);
        if (s != null)
        {
            s.IsActive = false;
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}