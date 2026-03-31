using AbuAmenPharma.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Admin")]
public class AdminUsersController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AdminUsersController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    // قائمة المستخدمين
    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users
            .OrderBy(u => u.Email)
            .ToListAsync();

        return View(users);
    }

    // إنشاء مستخدم
    public async Task<IActionResult> Create()
    {
        await EnsureRolesExist();
        ViewBag.Role = new SelectList(new[] { "Admin", "Operator" });
        return View(new AdminUserCreateVM());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminUserCreateVM vm)
    {
        await EnsureRolesExist();
        ViewBag.Role = new SelectList(new[] { "Admin", "Operator" }, vm.Role);

        if (!ModelState.IsValid) return View(vm);

        var email = vm.Email.Trim().ToLower();
        var exists = await _userManager.FindByEmailAsync(email);
        if (exists != null)
        {
            ModelState.AddModelError(nameof(vm.Email), "هذا البريد موجود بالفعل.");
            return View(vm);
        }

        var user = new IdentityUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        var createResult = await _userManager.CreateAsync(user, vm.Password);
        if (!createResult.Succeeded)
        {
            foreach (var e in createResult.Errors)
                ModelState.AddModelError("", e.Description);
            return View(vm);
        }

        // إضافة الدور
        var roleResult = await _userManager.AddToRoleAsync(user, vm.Role);
        if (!roleResult.Succeeded)
        {
            foreach (var e in roleResult.Errors)
                ModelState.AddModelError("", e.Description);
            return View(vm);
        }

        return RedirectToAction(nameof(Index));
    }

    // تعطيل/تفعيل (Lockout)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleLock(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        // لا تعطل نفسك بالخطأ
        if (user.Id == _userManager.GetUserId(User))
            return BadRequest("لا يمكن تعطيل المستخدم الحالي.");

        var locked = await _userManager.IsLockedOutAsync(user);

        if (!locked)
        {
            await _userManager.SetLockoutEnabledAsync(user, true);
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(50));
        }
        else
        {
            await _userManager.SetLockoutEndDateAsync(user, null);
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task EnsureRolesExist()
    {
        string[] roles = { "Admin", "Operator" };
        foreach (var r in roles)
            if (!await _roleManager.RoleExistsAsync(r))
                await _roleManager.CreateAsync(new IdentityRole(r));
    }
}
