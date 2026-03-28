using AbuAmenPharma.Data;
using AbuAmenPharma.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AbuAmenPharma.Controllers
{
    public class ManufacturersController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ManufacturersController(ApplicationDbContext context) => _context = context;

        public async Task<IActionResult> Index()
        => View(await _context.Manufacturers.Where(x => x.IsActive).OrderBy(x => x.NameAr).ToListAsync());

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Manufacturer manufacturer)
        {
            if (!ModelState.IsValid) return View(manufacturer);

            manufacturer.NameAr = manufacturer.NameAr.Trim();
            _context.Add(manufacturer);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var manufacturer = await _context.Manufacturers.FirstOrDefaultAsync(x => x.Id == id && x.IsActive);
            if (manufacturer == null) return NotFound();
            return View(manufacturer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Manufacturer manufacturer)
        {
            if (id != manufacturer.Id) return NotFound();
            if (!ModelState.IsValid) return View(manufacturer);

            var db = await _context.Manufacturers.FindAsync(id);
            if (db == null || !db.IsActive) return NotFound();

            db.NameAr = manufacturer.NameAr.Trim();
            db.Country = manufacturer.Country?.Trim();
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // شاشة تأكيد التعطيل
        public async Task<IActionResult> Disable(int? id)
        {
            if (id == null) return NotFound();
            var manufacturer = await _context.Manufacturers.FirstOrDefaultAsync(x => x.Id == id && x.IsActive);
            if (manufacturer == null) return NotFound();
            return View(manufacturer);
        }

        [HttpPost, ActionName("Disable")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DisableConfirmed(int id)
        {
            var manufacturer = await _context.Manufacturers.FindAsync(id);
            if (manufacturer != null)
            {
                manufacturer.IsActive = false;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
