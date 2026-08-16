using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartSociety.Data;
using SmartSociety.Models;
using SmartSociety.Models.Enums;
using SmartSociety.Services;
using System.Security.Claims;

namespace SmartSociety.Controllers
{
    [Authorize(Roles = "Admin,SecurityStaff")]
    public class SecurityController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuditService _audit;

        public SecurityController(AppDbContext context, UserManager<ApplicationUser> userManager, IAuditService audit)
        {
            _context = context;
            _userManager = userManager;
            _audit = audit;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;
            ViewBag.TodayVisitors = await _context.Visitors
                .Include(v => v.Flat)
                .Where(v => v.ValidFrom.Date <= today && v.ValidUntil >= today && v.IsApproved)
                .OrderBy(v => v.ValidFrom)
                .ToListAsync();

            ViewBag.ActiveLogs = await _context.GateLogs
                .Include(g => g.Visitor)
                .ThenInclude(v => v.Flat)
                .Where(g => g.Status == GateLogStatus.Entered && g.ExitTime == null)
                .OrderByDescending(g => g.EntryTime)
                .ToListAsync();

            return View();
        }

        [HttpGet]
        public IActionResult VerifyPass() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyPass(string gatePassCode, string actionType = "Entry")
        {
            gatePassCode = (gatePassCode ?? string.Empty).Trim();
            if (gatePassCode.Length < 6)
            {
                ModelState.AddModelError(nameof(gatePassCode), "Enter a valid gate pass code.");
                return View();
            }

            var visitor = await _context.Visitors
                .Include(v => v.Flat)
                .FirstOrDefaultAsync(v => v.GatePassCode == gatePassCode);

            if (visitor == null)
            {
                ModelState.AddModelError(string.Empty, "Gate pass not found.");
                return View();
            }

            var now = DateTime.Now;
            if (!visitor.IsApproved || now < visitor.ValidFrom || now > visitor.ValidUntil)
            {
                ModelState.AddModelError(string.Empty, "Pass is not valid, approved, or within its allowed time window.");
                return View(visitor);
            }

            var guard = await _userManager.GetUserAsync(User);
            if (guard == null) return Challenge();

            if (actionType.Equals("Exit", StringComparison.OrdinalIgnoreCase))
            {
                var active = await _context.GateLogs
                    .Where(g => g.VisitorId == visitor.Id && g.Status == GateLogStatus.Entered && g.ExitTime == null)
                    .OrderByDescending(g => g.EntryTime)
                    .FirstOrDefaultAsync();

                if (active == null)
                {
                    ModelState.AddModelError(string.Empty, "No active entry was found for this visitor.");
                    return View(visitor);
                }

                active.ExitTime = now;
                active.Status = GateLogStatus.Exited;
                await _context.SaveChangesAsync();
                await _audit.LogAsync(guard.Id, "Exit", "GateLog", active.Id.ToString(), $"Visitor:{visitor.VisitorName};GatePass:{visitor.GatePassCode}");
                TempData["Success"] = "Visitor checked out successfully.";
                return RedirectToAction(nameof(Index));
            }

            var alreadyInside = await _context.GateLogs.AnyAsync(g => g.VisitorId == visitor.Id && g.Status == GateLogStatus.Entered && g.ExitTime == null);
            if (alreadyInside)
            {
                ModelState.AddModelError(string.Empty, "Visitor is already checked in.");
                return View(visitor);
            }

            var log = new GateLog
            {
                VisitorId = visitor.Id,
                SecurityGuardId = guard.Id,
                EntryTime = now,
                Status = GateLogStatus.Entered
            };

            _context.GateLogs.Add(log);
            await _context.SaveChangesAsync();
            await _audit.LogAsync(guard.Id, "Entry", "GateLog", log.Id.ToString(), $"Visitor:{visitor.VisitorName};GatePass:{visitor.GatePassCode}");

            TempData["Success"] = "Visitor checked in successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
