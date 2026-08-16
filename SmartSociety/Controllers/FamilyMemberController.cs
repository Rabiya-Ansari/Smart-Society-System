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

        public FamilyMemberController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            IAuditService auditService)
        {
            _context = context;
            _userManager = userManager;
            _auditService = auditService;
        }


        // =========================================================
        // INDEX
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // ADMIN
            if (User.IsInRole("Admin"))
            {
                var members = await _context.FamilyMembers
                    .Include(f => f.ResidentProfile)
                    .ThenInclude(r => r.Flat)
                    .OrderByDescending(f => f.Id)
                    .ToListAsync();

                return View(members);
            }

            // RESIDENT
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            var resident = await _context.ResidentProfiles
                .FirstOrDefaultAsync(r =>
                    r.ApplicationUserId == user.Id);

            if (resident == null)
                return Forbid();

            var mine = await _context.FamilyMembers
                .Where(f => f.ResidentProfileId == resident.Id)
                .Include(f => f.ResidentProfile)
                .ThenInclude(r => r.Flat)
                .OrderByDescending(f => f.Id)
                .ToListAsync();

            return View(mine);
        }


        // =========================================================
        // DETAILS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var member = await _context.FamilyMembers
                .Include(f => f.ResidentProfile)
                .ThenInclude(r => r.Flat)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (member == null)
                return NotFound();

            // RESIDENT SECURITY CHECK
            if (!User.IsInRole("Admin"))
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                    return Challenge();

                if (member.ResidentProfile.ApplicationUserId != user.Id)
                    return Forbid();
            }

            return View(member);
        }


        // =========================================================
        // CREATE - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // ADMIN
            if (User.IsInRole("Admin"))
            {
                await LoadResidentsAsync();

                return View(new FamilyMember());
            }

            // RESIDENT
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            var resident = await _context.ResidentProfiles
                .FirstOrDefaultAsync(r =>
                    r.ApplicationUserId == user.Id);

            if (resident == null)
                return Forbid();

            var model = new FamilyMember
            {
                ResidentProfileId = resident.Id
            };

            return View(model);
        }


        // =========================================================
        // CREATE - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FamilyMember member)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
                return Challenge();


            // -----------------------------------------------------
            // RESIDENT
            // -----------------------------------------------------

            if (!User.IsInRole("Admin"))
            {
                var resident = await _context.ResidentProfiles
                    .FirstOrDefaultAsync(r =>
                        r.ApplicationUserId == currentUser.Id);

                if (resident == null)
                    return Forbid();

                // Resident apna ResidentProfileId khud submit nahi karega
                member.ResidentProfileId = resident.Id;
            }


            // -----------------------------------------------------
            // CHECK RESIDENT
            // -----------------------------------------------------

            var residentExists =
                await _context.ResidentProfiles
                    .AnyAsync(r =>
                        r.Id == member.ResidentProfileId);

            if (!residentExists)
            {
                ModelState.AddModelError(
                    nameof(member.ResidentProfileId),
                    "Please select a valid resident."
                );
            }


            // -----------------------------------------------------
            // VALIDATION
            // -----------------------------------------------------

            if (!ModelState.IsValid)
            {
                if (User.IsInRole("Admin"))
                {
                    await LoadResidentsAsync();
                }

                return View(member);
            }


            // -----------------------------------------------------
            // SAVE
            // -----------------------------------------------------

            try
            {
                _context.FamilyMembers.Add(member);

                await _context.SaveChangesAsync();

                await _auditService.LogAsync(
                    currentUser.Id,
                    "Create",
                    "FamilyMember",
                    member.Id.ToString(),
                    $"Name:{member.Name}"
                );

                TempData["Success"] =
                    "Family member created successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Unable to save family member. Please try again."
                );

                if (User.IsInRole("Admin"))
                {
                    await LoadResidentsAsync();
                }

                return View(member);
            }
        }


        // =========================================================
        // EDIT - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var member = await _context.FamilyMembers
                .FirstOrDefaultAsync(f => f.Id == id);

            if (member == null)
                return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
                return Challenge();


            // -----------------------------------------------------
            // RESIDENT SECURITY
            // -----------------------------------------------------

            if (!User.IsInRole("Admin"))
            {
                var resident = await _context.ResidentProfiles
                    .FirstOrDefaultAsync(r =>
                        r.ApplicationUserId == currentUser.Id);

                if (resident == null)
                    return Forbid();

                if (member.ResidentProfileId != resident.Id)
                    return Forbid();
            }


            // -----------------------------------------------------
            // ADMIN DROPDOWN
            // -----------------------------------------------------

            if (User.IsInRole("Admin"))
            {
                await LoadResidentsAsync();
            }

            return View(member);
        }


        // =========================================================
        // EDIT - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            FamilyMember member)
        {
            if (id != member.Id)
                return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
                return Challenge();


            // -----------------------------------------------------
            // GET EXISTING MEMBER
            // -----------------------------------------------------

            var existingMember = await _context.FamilyMembers
                .FirstOrDefaultAsync(f => f.Id == id);

            if (existingMember == null)
                return NotFound();


            // -----------------------------------------------------
            // RESIDENT SECURITY
            // -----------------------------------------------------

            if (!User.IsInRole("Admin"))
            {
                var resident = await _context.ResidentProfiles
                    .FirstOrDefaultAsync(r =>
                        r.ApplicationUserId == currentUser.Id);

                if (resident == null)
                    return Forbid();

                if (existingMember.ResidentProfileId != resident.Id)
                    return Forbid();

                // Resident ka ResidentProfileId change nahi hoga
                member.ResidentProfileId = resident.Id;
            }


            // -----------------------------------------------------
            // CHECK RESIDENT
            // -----------------------------------------------------

            var residentExists =
                await _context.ResidentProfiles
                    .AnyAsync(r =>
                        r.Id == member.ResidentProfileId);

            if (!residentExists)
            {
                ModelState.AddModelError(
                    nameof(member.ResidentProfileId),
                    "Please select a valid resident."
                );
            }


            // -----------------------------------------------------
            // VALIDATION
            // -----------------------------------------------------

            if (!ModelState.IsValid)
            {
                if (User.IsInRole("Admin"))
                {
                    await LoadResidentsAsync();
                }

                return View(member);
            }


            // -----------------------------------------------------
            // UPDATE ONLY ALLOWED FIELDS
            // -----------------------------------------------------

            existingMember.ResidentProfileId =
                member.ResidentProfileId;

            existingMember.Name =
                member.Name;

            existingMember.Relationship =
                member.Relationship;

            existingMember.DateOfBirth =
                member.DateOfBirth;


            // -----------------------------------------------------
            // SAVE
            // -----------------------------------------------------

            try
            {
                await _context.SaveChangesAsync();

                await _auditService.LogAsync(
                    currentUser.Id,
                    "Update",
                    "FamilyMember",
                    existingMember.Id.ToString(),
                    $"Name:{existingMember.Name}"
                );

                TempData["Success"] =
                    "Family member updated successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FamilyMemberExists(id))
                    return NotFound();

                throw;
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Unable to update family member. Please try again."
                );

                if (User.IsInRole("Admin"))
                {
                    await LoadResidentsAsync();
                }

                return View(member);
            }
        }


        // =========================================================
        // DELETE - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var member = await _context.FamilyMembers
                .Include(f => f.ResidentProfile)
                .ThenInclude(r => r.Flat)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (member == null)
                return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
                return Challenge();


            // -----------------------------------------------------
            // RESIDENT SECURITY
            // -----------------------------------------------------

            if (!User.IsInRole("Admin"))
            {
                if (member.ResidentProfile.ApplicationUserId
                    != currentUser.Id)
                {
                    return Forbid();
                }
            }

            return View(member);
        }


        // =========================================================
        // DELETE - POST
        // =========================================================

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var member = await _context.FamilyMembers
                .FirstOrDefaultAsync(f => f.Id == id);

            if (member == null)
                return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
                return Challenge();


            // -----------------------------------------------------
            // RESIDENT SECURITY
            // -----------------------------------------------------

            if (!User.IsInRole("Admin"))
            {
                var resident = await _context.ResidentProfiles
                    .FirstOrDefaultAsync(r =>
                        r.ApplicationUserId == currentUser.Id);

                if (resident == null)
                    return Forbid();

                if (member.ResidentProfileId != resident.Id)
                    return Forbid();
            }


            // -----------------------------------------------------
            // DELETE
            // -----------------------------------------------------

            try
            {
                _context.FamilyMembers.Remove(member);

                await _context.SaveChangesAsync();

                await _auditService.LogAsync(
                    currentUser.Id,
                    "Delete",
                    "FamilyMember",
                    member.Id.ToString(),
                    $"Name:{member.Name}"
                );

                TempData["Success"] =
                    "Family member deleted successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                TempData["Error"] =
                    "Unable to delete family member.";

                return RedirectToAction(nameof(Index));
            }
        }


        // =========================================================
        // LOAD RESIDENTS
        // =========================================================

        private async Task LoadResidentsAsync()
        {
            var residents = await _context.ResidentProfiles
                .Include(r => r.Flat)
                .OrderBy(r => r.FullName)
                .ToListAsync();

            var items = residents
                .Select(r => new SelectListItem
                {
                    Value = r.Id.ToString(),

                    Text = r.Flat == null
                        ? r.FullName
                        : $"{r.FullName} ({r.Flat.BlockName}-{r.Flat.FlatNumber})"
                })
                .ToList();

            ViewBag.Residents = items;
        }


        // =========================================================
        // EXISTS
        // =========================================================

        private bool FamilyMemberExists(int id)
        {
            return _context.FamilyMembers
                .Any(f => f.Id == id);
        }
    }
}