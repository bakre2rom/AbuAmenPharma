using AbuAmenPharma.Data;
using AbuAmenPharma.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Admin")]
public class SuppliersController : Controller
{
    private readonly ApplicationDbContext _context;
    public SuppliersController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index()
        => View(await _context.Suppliers.Where(x => x.IsActive).OrderBy(x => x.Name).ToListAsync());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string name, string? phone)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            TempData["ErrorMessage"] = "اسم المورد مطلوب";
            return RedirectToAction(nameof(Index));
        }

        _context.Suppliers.Add(new Supplier
        {
            Name = name.Trim(),
            Phone = phone?.Trim(),
            IsActive = true
        });
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "تم إضافة المورد بنجاح";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, string name, string? phone)
    {
        var supplier = await _context.Suppliers.FindAsync(id);
        if (supplier == null) return NotFound();

        if (string.IsNullOrWhiteSpace(name))
        {
            TempData["ErrorMessage"] = "اسم المورد مطلوب";
            return RedirectToAction(nameof(Index));
        }

        supplier.Name = name.Trim();
        supplier.Phone = phone?.Trim();
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "تم تعديل المورد بنجاح";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Disable(int id)
    {
        var supplier = await _context.Suppliers.FindAsync(id);
        if (supplier != null)
        {
            supplier.IsActive = false;
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "تم تعطيل المورد بنجاح";
        }
        return RedirectToAction(nameof(Index));
    }
}
