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
    public class EmergencyContactController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuditService _auditService;

        public EmergencyContactController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            IAuditService auditService)
        {
            _context = context;
            _userManager = userManager;
            _auditService = auditService;
        }


        // =====================================================
        // INDEX
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Admin"))
            {
                var contacts = await _context.EmergencyContacts
                    .Include(e => e.ResidentProfile)
                    .ThenInclude(r => r.Flat)
                    .OrderByDescending(e => e.Id)
                    .ToListAsync();

                return View(contacts);
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            var resident = await _context.ResidentProfiles
                .FirstOrDefaultAsync(r =>
                    r.ApplicationUserId == user.Id);

            if (resident == null)
                return Forbid();

            var contactsMine = await _context.EmergencyContacts
                .Where(e => e.ResidentProfileId == resident.Id)
                .Include(e => e.ResidentProfile)
                .ThenInclude(r => r.Flat)
                .OrderByDescending(e => e.Id)
                .ToListAsync();

            return View(contactsMine);
        }


        // =====================================================
        // DETAILS GET
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var contact = await _context.EmergencyContacts
                .Include(e => e.ResidentProfile)
                .ThenInclude(r => r.Flat)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (contact == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            if (!User.IsInRole("Admin") &&
                contact.ResidentProfile.ApplicationUserId != user.Id)
            {
                return Forbid();
            }

            return View(contact);
        }


        // =====================================================
        // CREATE GET
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // ADMIN
            if (User.IsInRole("Admin"))
            {
                await LoadResidentsAsync();

                return View(new EmergencyContact());
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

            var model = new EmergencyContact
            {
                ResidentProfileId = resident.Id
            };

            return View(model);
        }


        // =====================================================
        // CREATE POST
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmergencyContact contact)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
                return Challenge();


            // -----------------------------------------------
            // RESIDENT USER
            // -----------------------------------------------

            if (!User.IsInRole("Admin"))
            {
                var resident = await _context.ResidentProfiles
                    .FirstOrDefaultAsync(r =>
                        r.ApplicationUserId == currentUser.Id);

                if (resident == null)
                    return Forbid();

                // Controller automatically sets resident
                contact.ResidentProfileId = resident.Id;
            }


            // -----------------------------------------------
            // CHECK RESIDENT
            // -----------------------------------------------

            var residentExists =
                await _context.ResidentProfiles
                    .AnyAsync(r =>
                        r.Id == contact.ResidentProfileId);

            if (!residentExists)
            {
                ModelState.AddModelError(
                    nameof(contact.ResidentProfileId),
                    "Please select a valid resident."
                );
            }


            // -----------------------------------------------
            // MODEL VALIDATION
            // -----------------------------------------------

            if (!ModelState.IsValid)
            {
                if (User.IsInRole("Admin"))
                {
                    await LoadResidentsAsync();
                }

                return View(contact);
            }


            // -----------------------------------------------
            // SAVE
            // -----------------------------------------------

            try
            {
                _context.EmergencyContacts.Add(contact);

                await _context.SaveChangesAsync();


                await _auditService.LogAsync(
                    currentUser.Id,
                    "Create",
                    "EmergencyContact",
                    contact.Id.ToString(),
                    $"Name:{contact.Name}"
                );


                TempData["Success"] =
                    "Emergency contact created successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Unable to save emergency contact. Please try again."
                );

                if (User.IsInRole("Admin"))
                {
                    await LoadResidentsAsync();
                }

                return View(contact);
            }
        }


        // =====================================================
        // EDIT GET
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();


            var contact = await _context.EmergencyContacts
                .FirstOrDefaultAsync(e => e.Id == id);

            if (contact == null)
                return NotFound();


            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
                return Challenge();


            // -----------------------------------------------
            // RESIDENT USER SECURITY CHECK
            // -----------------------------------------------

            if (!User.IsInRole("Admin"))
            {
                var resident = await _context.ResidentProfiles
                    .FirstOrDefaultAsync(r =>
                        r.ApplicationUserId == currentUser.Id);

                if (resident == null)
                    return Forbid();

                if (contact.ResidentProfileId != resident.Id)
                    return Forbid();
            }


            // -----------------------------------------------
            // IMPORTANT
            // Load residents for Admin dropdown
            // -----------------------------------------------

            if (User.IsInRole("Admin"))
            {
                await LoadResidentsAsync();
            }


            return View(contact);
        }


        // =====================================================
        // EDIT POST
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            EmergencyContact contact)
        {
            if (id != contact.Id)
                return NotFound();


            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
                return Challenge();


            // -----------------------------------------------
            // RESIDENT USER
            // -----------------------------------------------

            if (!User.IsInRole("Admin"))
            {
                var resident = await _context.ResidentProfiles
                    .FirstOrDefaultAsync(r =>
                        r.ApplicationUserId == currentUser.Id);

                if (resident == null)
                    return Forbid();

                contact.ResidentProfileId = resident.Id;
            }


            // -----------------------------------------------
            // RESIDENT EXISTS
            // -----------------------------------------------

            var residentExists =
                await _context.ResidentProfiles
                    .AnyAsync(r =>
                        r.Id == contact.ResidentProfileId);

            if (!residentExists)
            {
                ModelState.AddModelError(
                    nameof(contact.ResidentProfileId),
                    "Please select a valid resident."
                );
            }


            // -----------------------------------------------
            // VALIDATION
            // -----------------------------------------------

            if (!ModelState.IsValid)
            {
                if (User.IsInRole("Admin"))
                {
                    await LoadResidentsAsync();
                }

                return View(contact);
            }


            // -----------------------------------------------
            // UPDATE
            // -----------------------------------------------

            try
            {
                var existingContact =
                    await _context.EmergencyContacts
                        .FirstOrDefaultAsync(e => e.Id == id);

                if (existingContact == null)
                    return NotFound();


                existingContact.ResidentProfileId =
                    contact.ResidentProfileId;

                existingContact.Name =
                    contact.Name;

                existingContact.Relationship =
                    contact.Relationship;

                existingContact.PhoneNumber =
                    contact.PhoneNumber;


                await _context.SaveChangesAsync();


                await _auditService.LogAsync(
                    currentUser.Id,
                    "Update",
                    "EmergencyContact",
                    existingContact.Id.ToString(),
                    $"Name:{existingContact.Name}"
                );


                TempData["Success"] =
                    "Emergency contact updated successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmergencyContactExists(id))
                    return NotFound();

                throw;
            }
        }


        // =====================================================
        // DELETE GET
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();


            var contact = await _context.EmergencyContacts
                .Include(e => e.ResidentProfile)
                .ThenInclude(r => r.Flat)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (contact == null)
                return NotFound();


            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();


            if (!User.IsInRole("Admin") &&
                contact.ResidentProfile.ApplicationUserId != user.Id)
            {
                return Forbid();
            }


            return View(contact);
        }


        // =====================================================
        // DELETE POST
        // =====================================================

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contact =
                await _context.EmergencyContacts
                    .FirstOrDefaultAsync(e => e.Id == id);

            if (contact == null)
                return NotFound();


            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();


            // -----------------------------------------------
            // RESIDENT SECURITY
            // -----------------------------------------------

            if (!User.IsInRole("Admin"))
            {
                var resident =
                    await _context.ResidentProfiles
                        .FirstOrDefaultAsync(r =>
                            r.ApplicationUserId == user.Id);

                if (resident == null)
                    return Forbid();

                if (contact.ResidentProfileId != resident.Id)
                    return Forbid();
            }


            // -----------------------------------------------
            // DELETE
            // -----------------------------------------------

            _context.EmergencyContacts.Remove(contact);

            await _context.SaveChangesAsync();


            await _auditService.LogAsync(
                user.Id,
                "Delete",
                "EmergencyContact",
                contact.Id.ToString(),
                $"Name:{contact.Name}"
            );


            TempData["Success"] =
                "Emergency contact deleted successfully.";

            return RedirectToAction(nameof(Index));
        }


        // =====================================================
        // LOAD RESIDENTS
        // =====================================================

        private async Task LoadResidentsAsync()
        {
            var residents = await _context.ResidentProfiles
                .Include(r => r.Flat)
                .OrderBy(r => r.FullName)
                .ToListAsync();


            ViewBag.Residents = residents
                .Select(r => new SelectListItem
                {
                    Value = r.Id.ToString(),

                    Text = r.Flat == null
                        ? r.FullName
                        : $"{r.FullName} ({r.Flat.BlockName}-{r.Flat.FlatNumber})"

                })
                .ToList();
        }


        // =====================================================
        // EXISTS
        // =====================================================

        private bool EmergencyContactExists(int id)
        {
            return _context.EmergencyContacts
                .Any(e => e.Id == id);
        }
    }
}