using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartSociety.Data;
using SmartSociety.Models;
using SmartSociety.Models.ViewModels;

namespace SmartSociety.Controllers
{
    [Authorize(Roles = "Admin,Resident")]
    public class ResidentController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ResidentController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        // =========================================================
        // GET: Resident/Create
        // =========================================================

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            await LoadFlatsAsync();

            return View();
        }


        // =========================================================
        // POST: Resident/Create
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(
            ResidentRegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadFlatsAsync();
                return View(model);
            }


            // -----------------------------------------------------
            // Check duplicate CNIC
            // -----------------------------------------------------

            bool cnicExists = await _context.ResidentProfiles
                .AnyAsync(r => r.CNIC == model.CNIC);

            if (cnicExists)
            {
                ModelState.AddModelError(
                    nameof(model.CNIC),
                    "This CNIC is already registered."
                );

                await LoadFlatsAsync();
                return View(model);
            }


            // -----------------------------------------------------
            // Check selected Flat
            // -----------------------------------------------------

            var flat = await _context.Flats
                .FirstOrDefaultAsync(f => f.Id == model.FlatId);

            if (flat == null)
            {
                ModelState.AddModelError(
                    nameof(model.FlatId),
                    "Selected flat does not exist."
                );

                await LoadFlatsAsync();
                return View(model);
            }


            // -----------------------------------------------------
            // Check if Flat is already occupied
            // -----------------------------------------------------

            if (flat.IsOccupied)
            {
                ModelState.AddModelError(
                    nameof(model.FlatId),
                    "This flat is already occupied."
                );

                await LoadFlatsAsync();
                return View(model);
            }


            // -----------------------------------------------------
            // Check duplicate Email
            // -----------------------------------------------------

            var existingUser = await _userManager
                .FindByEmailAsync(model.Email);

            if (existingUser != null)
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "This email is already registered."
                );

                await LoadFlatsAsync();
                return View(model);
            }


            // =====================================================
            // Create Identity User
            // =====================================================

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                EmailConfirmed = true
            };


            var result = await _userManager.CreateAsync(
                user,
                model.Password
            );


            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description
                    );
                }

                await LoadFlatsAsync();
                return View(model);
            }


            // =====================================================
            // Assign Resident Role
            // =====================================================

            var roleResult = await _userManager.AddToRoleAsync(
                user,
                "Resident"
            );


            if (!roleResult.Succeeded)
            {
                foreach (var error in roleResult.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description
                    );
                }

                await _userManager.DeleteAsync(user);

                await LoadFlatsAsync();
                return View(model);
            }


            // =====================================================
            // Create Resident Profile
            // =====================================================

            var resident = new ResidentProfile
            {
                ApplicationUserId = user.Id,
                FullName = model.FullName,
                CNIC = model.CNIC,
                FlatId = model.FlatId
            };


            _context.ResidentProfiles.Add(resident);


            // =====================================================
            // Mark Flat as Occupied
            // =====================================================

            flat.IsOccupied = true;


            await _context.SaveChangesAsync();


            TempData["Success"] =
                "Resident registered successfully.";


            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // GET: Resident/Index
        // =========================================================

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var residents = await _context.ResidentProfiles
                .Include(r => r.ApplicationUser)
                .Include(r => r.Flat)
                .ToListAsync();

            return View(residents);
        }


        // =========================================================
        // GET: Resident/Details/5
        // =========================================================

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }


            var resident = await _context.ResidentProfiles
                .Include(r => r.ApplicationUser)
                .Include(r => r.Flat)
                .Include(r => r.Vehicles)
                .Include(r => r.EmergencyContacts)
                .Include(r => r.FamilyMembers)
                .Include(r => r.Complaints)
                .Include(r => r.AmenityBookings)
                .FirstOrDefaultAsync(r => r.Id == id);


            if (resident == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Resident"))
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null || resident.ApplicationUserId != currentUser.Id)
                    return Forbid();
            }

            return View(resident);
        }


        // =========================================================
        // GET: Resident/Edit/5
        // =========================================================

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }


            var resident = await _context.ResidentProfiles
                .Include(r => r.ApplicationUser)
                .Include(r => r.Flat)
                .FirstOrDefaultAsync(r => r.Id == id);


            if (resident == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Resident"))
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null || resident.ApplicationUserId != currentUser.Id)
                    return Forbid();
            }

            // Load current flat + available flats

            await LoadFlatsForEditAsync(resident.FlatId);


            var model = new ResidentEditViewModel
            {
                Id = resident.Id,

                FullName = resident.FullName,

                CNIC = resident.CNIC,

                Email = resident.ApplicationUser.Email
                    ?? string.Empty,

                PhoneNumber = resident.ApplicationUser.PhoneNumber
                    ?? string.Empty,

                FlatId = resident.FlatId
            };


            return View(model);
        }


        // =========================================================
        // POST: Resident/Edit/5
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            ResidentEditViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }


            // -----------------------------------------------------
            // Model validation
            // -----------------------------------------------------

            if (!ModelState.IsValid)
            {
                await LoadFlatsForEditAsync(model.FlatId);

                return View(model);
            }


            // -----------------------------------------------------
            // Get Resident
            // -----------------------------------------------------

            var resident = await _context.ResidentProfiles
                .Include(r => r.ApplicationUser)
                .Include(r => r.Flat)
                .FirstOrDefaultAsync(r => r.Id == id);


            if (resident == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Resident"))
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null || resident.ApplicationUserId != currentUser.Id)
                    return Forbid();
                model.FlatId = resident.FlatId;
            }

            // -----------------------------------------------------
            // Check duplicate CNIC
            // -----------------------------------------------------

            bool cnicExists = await _context.ResidentProfiles
                .AnyAsync(r =>
                    r.Id != id &&
                    r.CNIC == model.CNIC);


            if (cnicExists)
            {
                ModelState.AddModelError(
                    nameof(model.CNIC),
                    "This CNIC is already registered."
                );

                await LoadFlatsForEditAsync(model.FlatId);

                return View(model);
            }


            // -----------------------------------------------------
            // Check duplicate Email
            // -----------------------------------------------------

            bool emailExists = await _context.Users
                .AnyAsync(u =>
                    u.Id != resident.ApplicationUserId &&
                    u.Email == model.Email);


            if (emailExists)
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "This email is already registered."
                );

                await LoadFlatsForEditAsync(model.FlatId);

                return View(model);
            }


            // -----------------------------------------------------
            // Get selected Flat
            // -----------------------------------------------------

            var newFlat = await _context.Flats
                .FirstOrDefaultAsync(f => f.Id == model.FlatId);


            if (newFlat == null)
            {
                ModelState.AddModelError(
                    nameof(model.FlatId),
                    "Selected flat does not exist."
                );

                await LoadFlatsForEditAsync(model.FlatId);

                return View(model);
            }


            // =====================================================
            // Flat Change Logic
            // =====================================================

            if (resident.FlatId != model.FlatId)
            {
                // New flat must be available

                if (newFlat.IsOccupied)
                {
                    ModelState.AddModelError(
                        nameof(model.FlatId),
                        "Selected flat is already occupied."
                    );

                    await LoadFlatsForEditAsync(model.FlatId);

                    return View(model);
                }


                // Old flat becomes available

                resident.Flat.IsOccupied = false;


                // New flat becomes occupied

                newFlat.IsOccupied = true;


                // Assign new flat

                resident.FlatId = model.FlatId;
            }


            // =====================================================
            // Update Resident Profile
            // =====================================================

            resident.FullName = model.FullName;

            resident.CNIC = model.CNIC;


            // =====================================================
            // Update Identity User
            // =====================================================

            var emailResult = await _userManager.SetEmailAsync(resident.ApplicationUser, model.Email);
            if (!emailResult.Succeeded)
            {
                foreach (var error in emailResult.Errors)
                    ModelState.AddModelError(nameof(model.Email), error.Description);
                await LoadFlatsForEditAsync(model.FlatId);
                return View(model);
            }

            var usernameResult = await _userManager.SetUserNameAsync(resident.ApplicationUser, model.Email);
            if (!usernameResult.Succeeded)
            {
                foreach (var error in usernameResult.Errors)
                    ModelState.AddModelError(nameof(model.Email), error.Description);
                await LoadFlatsForEditAsync(model.FlatId);
                return View(model);
            }

            resident.ApplicationUser.PhoneNumber = model.PhoneNumber;

            // =====================================================
            // Save Changes
            // =====================================================

            await _context.SaveChangesAsync();


            TempData["Success"] =
                "Resident updated successfully.";


            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // GET: Resident/Delete/5
        // =========================================================

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }


            var resident = await _context.ResidentProfiles
                .Include(r => r.ApplicationUser)
                .Include(r => r.Flat)
                .FirstOrDefaultAsync(r => r.Id == id);


            if (resident == null)
            {
                return NotFound();
            }


            return View(resident);
        }


        // =========================================================
        // POST: Resident/Delete/5
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // -----------------------------------------------------
            // Get resident with related data
            // -----------------------------------------------------

            var resident = await _context.ResidentProfiles
                .Include(r => r.ApplicationUser)
                .Include(r => r.Flat)
                .Include(r => r.Vehicles)
                .Include(r => r.EmergencyContacts)
                .Include(r => r.FamilyMembers)
                .Include(r => r.Complaints)
                .Include(r => r.AmenityBookings)
                .FirstOrDefaultAsync(r => r.Id == id);


            if (resident == null)
            {
                return NotFound();
            }


            // -----------------------------------------------------
            // Prevent delete if resident has related records
            // -----------------------------------------------------

            bool hasRelatedData =
                resident.Vehicles.Any() ||
                resident.EmergencyContacts.Any() ||
                resident.FamilyMembers.Any() ||
                resident.Complaints.Any() ||
                resident.AmenityBookings.Any();


            if (hasRelatedData)
            {
                TempData["Error"] =
                    "This resident cannot be deleted because related records exist. Remove related records first.";

                return RedirectToAction(nameof(Index));
            }


            // -----------------------------------------------------
            // Store values before deleting
            // -----------------------------------------------------

            var user = resident.ApplicationUser;

            var flat = resident.Flat;


            // -----------------------------------------------------
            // Release Flat
            // -----------------------------------------------------

            flat.IsOccupied = false;


            // -----------------------------------------------------
            // Delete Resident Profile
            // -----------------------------------------------------

            _context.ResidentProfiles.Remove(resident);


            await _context.SaveChangesAsync();


            // -----------------------------------------------------
            // Delete Identity User
            // -----------------------------------------------------

            if (user != null)
            {
                var userResult = await _userManager.DeleteAsync(user);

                if (!userResult.Succeeded)
                {
                    foreach (var error in userResult.Errors)
                    {
                        ModelState.AddModelError(
                            string.Empty,
                            error.Description
                        );
                    }

                    TempData["Error"] =
                        "Resident profile was deleted, but Identity user could not be deleted.";

                    return RedirectToAction(nameof(Index));
                }
            }


            TempData["Success"] =
                "Resident deleted successfully.";


            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // Load Available Flats
        // =========================================================

        private async Task LoadFlatsAsync()
        {
            var flats = await _context.Flats
                .Where(f => !f.IsOccupied)
                .OrderBy(f => f.BlockName)
                .ThenBy(f => f.FlatNumber)
                .ToListAsync();


            ViewBag.Flats = new SelectList(
                flats,
                "Id",
                "FlatNumber"
            );
        }


        // =========================================================
        // Load Flats For Edit
        // =========================================================

        private async Task LoadFlatsForEditAsync(
            int currentFlatId)
        {
            var flats = await _context.Flats
                .Where(f =>
                    !f.IsOccupied ||
                    f.Id == currentFlatId)
                .OrderBy(f => f.BlockName)
                .ThenBy(f => f.FlatNumber)
                .ToListAsync();


            ViewBag.Flats = new SelectList(
                flats,
                "Id",
                "FlatNumber",
                currentFlatId
            );
        }
    }
}