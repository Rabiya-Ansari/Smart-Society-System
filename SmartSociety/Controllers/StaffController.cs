using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartSociety.Data;
using SmartSociety.Models;
using SmartSociety.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartSociety.Controllers
{
    [Authorize(Roles = "Admin")]
    public class StaffController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IAuditService _audit;

        public StaffController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IAuditService audit)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _audit = audit;
        }

        public async Task<IActionResult> Index(string role = null)
        {
            var users = _userManager.Users.ToList();
            var model = new List<StaffListItemViewModel>();

            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                if (!string.IsNullOrEmpty(role) && !roles.Contains(role)) continue;

                model.Add(new StaffListItemViewModel
                {
                    Id = u.Id,
                    Email = u.Email,
                    FullName = u.FullName,
                    PhoneNumber = u.PhoneNumber,
                    Roles = roles,
                    IsActive = u.IsActive
                });
            }

            ViewBag.FilterRole = role;
            return View(model);
        }

        [HttpGet]
        public IActionResult Create(string role = "")
        {
            var vm = new StaffCreateViewModel { Role = role };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StaffCreateViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var existing = await _userManager.FindByEmailAsync(model.Email);
            if (existing != null)
            {
                ModelState.AddModelError(nameof(model.Email), "A user with this email already exists.");
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true,
                PhoneNumber = model.PhoneNumber,
                FullName = model.FullName,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors) ModelState.AddModelError(string.Empty, e.Description);
                return View(model);
            }

            if (!string.IsNullOrEmpty(model.Role) && await _roleManager.RoleExistsAsync(model.Role))
            {
                await _userManager.AddToRoleAsync(user, model.Role);
            }

            await _audit.LogAsync(user.Id, "Create", "Staff", user.Id, $"Created staff {user.Email} role={model.Role}");

            TempData["Success"] = "Staff account created.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            var roles = await _userManager.GetRolesAsync(user);
            var vm = new StaffDetailsViewModel
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Roles = roles,
                IsActive = user.IsActive
            };
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            var roles = await _userManager.GetRolesAsync(user);
            var vm = new StaffEditViewModel
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Role = roles.FirstOrDefault() ?? string.Empty,
                IsActive = user.IsActive
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(StaffEditViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound();

            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            user.IsActive = model.IsActive;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var e in updateResult.Errors) ModelState.AddModelError(string.Empty, e.Description);
                return View(model);
            }

            var roles = await _userManager.GetRolesAsync(user);
            var desiredRole = model.Role ?? string.Empty;
            if (!string.IsNullOrEmpty(desiredRole) && !(await _roleManager.RoleExistsAsync(desiredRole)))
            {
                ModelState.AddModelError(nameof(model.Role), "Selected role does not exist.");
                return View(model);
            }

            if (!roles.Contains(desiredRole))
            {
                if (roles.Any()) await _userManager.RemoveFromRolesAsync(user, roles);
                if (!string.IsNullOrEmpty(desiredRole)) await _userManager.AddToRoleAsync(user, desiredRole);
            }

            await _audit.LogAsync(User?.Identity?.Name ?? "system", "Edit", "Staff", user.Id, $"Edited staff {user.Email}");

            TempData["Success"] = "Staff updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            user.IsActive = false;
            await _userManager.UpdateAsync(user);
            await _audit.LogAsync(User?.Identity?.Name ?? "system", "Deactivate", "Staff", user.Id, $"Deactivated staff {user.Email}");
            TempData["Success"] = "Staff deactivated.";
            return RedirectToAction(nameof(Index));
        }

        // View models
        public class StaffListItemViewModel
        {
            public string Id { get; set; } = default!;
            public string? FullName { get; set; }
            public string? Email { get; set; }
            public string? PhoneNumber { get; set; }
            public IEnumerable<string> Roles { get; set; } = Enumerable.Empty<string>();
            public bool IsActive { get; set; }
        }

        public class StaffCreateViewModel
        {
            public string? FullName { get; set; }
            public string Email { get; set; } = string.Empty;
            public string? PhoneNumber { get; set; }
            public string Password { get; set; } = string.Empty;
            public string ConfirmPassword { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
        }

        public class StaffDetailsViewModel
        {
            public string Id { get; set; } = default!;
            public string? FullName { get; set; }
            public string? Email { get; set; }
            public string? PhoneNumber { get; set; }
            public IEnumerable<string> Roles { get; set; } = Enumerable.Empty<string>();
            public bool IsActive { get; set; }
        }

        public class StaffEditViewModel
        {
            public string Id { get; set; } = default!;
            public string? FullName { get; set; }
            public string Email { get; set; } = string.Empty;
            public string? PhoneNumber { get; set; }
            public string Role { get; set; } = string.Empty;
            public bool IsActive { get; set; }
        }
    }
}
