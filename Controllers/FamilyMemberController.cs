using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartSociety.Data;
using SmartSociety.Models;
using SmartSociety.Services;

namespace SmartSociety.Controllers
{
    [Authorize]
    public class FamilyMemberController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuditService _auditService;

        public FamilyMemberController(AppDbContext context, UserManager<ApplicationUser> userManager, IAuditService auditService)
        {
            _context = context;
            _userManager = userManager;
            _auditService = auditService;
        }

        // GET: FamilyMember
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Admin"))
            {
                var members = await _context.FamilyMembers
                    .Include(f => f.ResidentProfile)
                    .ThenInclude(r => r.Flat)
                    .ToListAsync();

                return View(members);
            }

            var user = await _userManager.GetUserAsync(User);
            var resident = await _context.ResidentProfiles.FirstOrDefaultAsync(r => r.ApplicationUserId == user.Id);
            if (resident == null) return Forbid();

            var mine = await _context.FamilyMembers
                .Where(f => f.ResidentProfileId == resident.Id)
                .Include(f => f.ResidentProfile)
                .ThenInclude(r => r.Flat)
                .ToListAsync();

            return View(mine);
        }

        // GET: FamilyMember/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var member = await _context.FamilyMembers
                .Include(f => f.ResidentProfile)
                .ThenInclude(r => r.Flat)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (member == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (!User.IsInRole("Admin") && member.ResidentProfile.ApplicationUserId != user.Id)
                return Forbid();

            return View(member);
        }

        // GET: FamilyMember/Create
        public async Task<IActionResult> Create()
        {
            if (User.IsInRole("Admin"))
            {
                await LoadResidentsAsync();
                return View();
            }

            var user = await _userManager.GetUserAsync(User);
            var resident = await _context.ResidentProfiles.FirstOrDefaultAsync(r => r.ApplicationUserId == user.Id);
            if (resident == null) return Forbid();

            var model = new FamilyMember { ResidentProfileId = resident.Id };
            return View(model);
        }

        // POST: FamilyMember/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FamilyMember member)
        {
            if (!ModelState.IsValid)
            {
                if (User.IsInRole("Admin")) await LoadResidentsAsync();
                return View(member);
            }
            var currentUser = await _userManager.GetUserAsync(User);

            if (!User.IsInRole("Admin"))
            {
                var resident = await _context.ResidentProfiles.FirstOrDefaultAsync(r => r.ApplicationUserId == currentUser.Id);
                if (resident == null) return Forbid();
                member.ResidentProfileId = resident.Id;
            }

            var residentExists = await _context.ResidentProfiles.AnyAsync(r => r.Id == member.ResidentProfileId);
            if (!residentExists)
            {
                ModelState.AddModelError(nameof(member.ResidentProfileId), "Selected resident does not exist.");
                if (User.IsInRole("Admin")) await LoadResidentsAsync();
                return View(member);
            }

            _context.FamilyMembers.Add(member);

            try
            {
                await _context.SaveChangesAsync();
                await _auditService.LogAsync(currentUser.Id, "Create", "FamilyMember", member.Id.ToString(), $"Name:{member.Name}");
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "Unable to save changes. Try again later.");
                if (User.IsInRole("Admin")) await LoadResidentsAsync();
                return View(member);
            }

            TempData["Success"] = "Family member created successfully.";

            return RedirectToAction(nameof(Index));
        }

        // GET: FamilyMember/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var member = await _context.FamilyMembers
                .FirstOrDefaultAsync(f => f.Id == id);

            if (member == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (!User.IsInRole("Admin") && member.ResidentProfileId != (await _context.ResidentProfiles.FirstOrDefaultAsync(r => r.ApplicationUserId == user.Id))?.Id)
                return Forbid();

            if (User.IsInRole("Admin")) await LoadResidentsAsync();
            return View(member);
        }

        // POST: FamilyMember/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FamilyMember member)
        {
            if (id != member.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                if (User.IsInRole("Admin")) await LoadResidentsAsync();
                return View(member);
            }
            var currentUser = await _userManager.GetUserAsync(User);

            if (!User.IsInRole("Admin"))
            {
                var resident = await _context.ResidentProfiles.FirstOrDefaultAsync(r => r.ApplicationUserId == currentUser.Id);
                if (resident == null) return Forbid();
                member.ResidentProfileId = resident.Id;
            }

            var residentExists = await _context.ResidentProfiles.AnyAsync(r => r.Id == member.ResidentProfileId);
            if (!residentExists)
            {
                ModelState.AddModelError(nameof(member.ResidentProfileId), "Selected resident does not exist.");
                if (User.IsInRole("Admin")) await LoadResidentsAsync();
                return View(member);
            }

            try
            {
                _context.Update(member);
                await _context.SaveChangesAsync();

                await _auditService.LogAsync(currentUser.Id, "Update", "FamilyMember", member.Id.ToString(), $"Name:{member.Name}");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FamilyMemberExists(member.Id)) return NotFound();
                throw;
            }

            TempData["Success"] = "Family member updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        // GET: FamilyMember/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var member = await _context.FamilyMembers
                .Include(f => f.ResidentProfile)
                .ThenInclude(r => r.Flat)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (member == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (!User.IsInRole("Admin") && member.ResidentProfile.ApplicationUserId != user.Id)
                return Forbid();

            return View(member);
        }

        // POST: FamilyMember/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var member = await _context.FamilyMembers.FindAsync(id);

            if (member == null) return NotFound();
            var user = await _userManager.GetUserAsync(User);
            if (!User.IsInRole("Admin") && member.ResidentProfileId != (await _context.ResidentProfiles.FirstOrDefaultAsync(r => r.ApplicationUserId == user.Id))?.Id)
                return Forbid();

            _context.FamilyMembers.Remove(member);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(user.Id, "Delete", "FamilyMember", member.Id.ToString(), $"Name:{member.Name}");

            TempData["Success"] = "Family member deleted successfully.";

            return RedirectToAction(nameof(Index));
        }

        private bool FamilyMemberExists(int id)
            => _context.FamilyMembers.Any(f => f.Id == id);

        private async Task LoadResidentsAsync()
        {
            var residents = await _context.ResidentProfiles
                .Include(r => r.Flat)
                .ToListAsync();

            var items = residents.Select(r => new SelectListItem
            {
                Value = r.Id.ToString(),
                Text = $"{r.FullName} ({r.Flat.BlockName}-{r.Flat.FlatNumber})"
            }).ToList();

            ViewBag.Residents = items;
        }
    }
}
