using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AbuAmenPharma.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RolesController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public RolesController(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            return View(roles);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                TempData["ErrorMessage"] = "اسم الصلاحية مطلوب.";
                return RedirectToAction(nameof(Index));
            }

            if (await _roleManager.RoleExistsAsync(roleName.Trim()))
            {
                TempData["ErrorMessage"] = "هذه الصلاحية موجودة بالفعل.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _roleManager.CreateAsync(new IdentityRole(roleName.Trim()));
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "تم إضافة الصلاحية بنجاح.";
            }
            else
            {
                TempData["ErrorMessage"] = string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();

            if (role.Name == "Admin")
            {
                TempData["ErrorMessage"] = "لا يمكن حذف صلاحيات المدير العام.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "تم حذف الصلاحية بنجاح.";
            }
            else
            {
                TempData["ErrorMessage"] = "فشل حذف الصلاحية.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
