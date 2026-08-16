using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartSociety.Data;
using SmartSociety.Models;
using SmartSociety.Models.Enums;

namespace SmartSociety.Controllers
{
    [Authorize(Roles = "Admin,SecurityStaff")]
    public class GateLogController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public GateLogController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ============================
        // INDEX
        // ============================
        public async Task<IActionResult> Index()
        {
            var logs = await _context.GateLogs
                .Include(g => g.Visitor)
                    .ThenInclude(v => v.Flat)
                .Include(g => g.SecurityGuard)
                .OrderByDescending(g => g.EntryTime)
                .ToListAsync();

            return View(logs);
        }


        // ============================
        // DETAILS
        // ============================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var log = await _context.GateLogs
                .Include(g => g.Visitor)
                    .ThenInclude(v => v.Flat)
                .Include(g => g.SecurityGuard)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (log == null)
                return NotFound();

            return View(log);
        }


        // ============================
        // CREATE - GET
        // ============================
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            await LoadLookups();

            return View();
        }


        // ============================
        // CREATE - POST
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(GateLog log)
        {
            if (!ModelState.IsValid)
            {
                await LoadLookups();
                return View(log);
            }

            // Check Visitor
            var visitorExists = await _context.Visitors
                .AnyAsync(v => v.Id == log.VisitorId);

            if (!visitorExists)
            {
                ModelState.AddModelError(
                    nameof(log.VisitorId),
                    "Selected visitor does not exist."
                );

                await LoadLookups();
                return View(log);
            }


            // Check Security Guard
            var guard = await _userManager
                .FindByIdAsync(log.SecurityGuardId);

            if (guard == null)
            {
                ModelState.AddModelError(
                    nameof(log.SecurityGuardId),
                    "Selected security guard does not exist."
                );

                await LoadLookups();
                return View(log);
            }


            // Check SecurityStaff Role
            var isSecurityStaff = await _userManager
                .IsInRoleAsync(guard, "SecurityStaff");

            if (!isSecurityStaff)
            {
                ModelState.AddModelError(
                    nameof(log.SecurityGuardId),
                    "Selected user is not a Security Staff member."
                );

                await LoadLookups();
                return View(log);
            }


            try
            {
                _context.GateLogs.Add(log);

                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "Gate Log created successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(
                    "",
                    "Gate Log could not be created: " + ex.Message
                );

                await LoadLookups();

                return View(log);
            }
        }


        // ============================
        // EDIT - GET
        // ============================
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var log = await _context.GateLogs
                .FindAsync(id);

            if (log == null)
                return NotFound();

            await LoadLookups();

            return View(log);
        }


        // ============================
        // EDIT - POST
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(
            int id,
            GateLog log)
        {
            if (id != log.Id)
                return NotFound();

            if (!ModelState.IsValid)
            {
                await LoadLookups();
                return View(log);
            }

            try
            {
                _context.GateLogs.Update(log);

                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "Gate Log updated successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(
                    "",
                    "Gate Log could not be updated: " + ex.Message
                );

                await LoadLookups();

                return View(log);
            }
        }


        // ============================
        // DELETE - GET
        // ============================
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var log = await _context.GateLogs
                .Include(g => g.Visitor)
                    .ThenInclude(v => v.Flat)
                .Include(g => g.SecurityGuard)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (log == null)
                return NotFound();

            return View(log);
        }


        // ============================
        // DELETE - POST
        // ============================
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var log = await _context.GateLogs
                .FindAsync(id);

            if (log == null)
                return NotFound();

            _context.GateLogs.Remove(log);

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Gate Log deleted successfully.";

            return RedirectToAction(nameof(Index));
        }


        // ============================
        // LOAD DROPDOWNS
        // ============================
        private async Task LoadLookups()
        {
            // Visitors
            var visitors = await _context.Visitors
                .Include(v => v.Flat)
                .OrderByDescending(v => v.ValidFrom)
                .ToListAsync();

            ViewBag.Visitors = new SelectList(
                visitors,
                "Id",
                "VisitorName"
            );


            // Security Guards
            var guards = await _userManager
                .GetUsersInRoleAsync("SecurityStaff");

            ViewBag.SecurityGuards = guards
                .Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = u.Email
                           ?? u.UserName
                           ?? u.Id
                })
                .ToList();


            // Status
            ViewBag.Statuses = new SelectList(
                Enum.GetValues<GateLogStatus>()
            );
        }
    }
}