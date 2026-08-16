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
    public class VehicleController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuditService _auditService;

        public VehicleController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            IAuditService auditService)
        {
            _context = context;
            _userManager = userManager;
            _auditService = auditService;
        }


        // =========================================================
        // GET: Vehicle
        // =========================================================

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            // Admin: show all vehicles
            if (User.IsInRole("Admin"))
            {
                var vehicles = await _context.Vehicles
                    .Include(v => v.ResidentProfile)
                    .ThenInclude(r => r.Flat)
                    .ToListAsync();

                return View(vehicles);
            }

            // Resident: show only own vehicles
            var resident = await _context.ResidentProfiles
                .FirstOrDefaultAsync(r =>
                    r.ApplicationUserId == user.Id);

            if (resident == null)
                return Forbid();

            var myVehicles = await _context.Vehicles
                .Where(v => v.ResidentProfileId == resident.Id)
                .Include(v => v.ResidentProfile)
                .ThenInclude(r => r.Flat)
                .ToListAsync();

            return View(myVehicles);
        }


        // =========================================================
        // GET: Vehicle/Details/5
        // =========================================================

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var vehicle = await _context.Vehicles
                .Include(v => v.ResidentProfile)
                .ThenInclude(r => r.Flat)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vehicle == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            // Admin can view all
            // Resident can view only own vehicle
            if (!User.IsInRole("Admin") &&
                vehicle.ResidentProfile.ApplicationUserId != user.Id)
            {
                return Forbid();
            }

            return View(vehicle);
        }


        // =========================================================
        // GET: Vehicle/Create
        // =========================================================

        public async Task<IActionResult> Create()
        {
            if (User.IsInRole("Admin"))
            {
                await LoadResidentsAsync();

                return View();
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            // Resident can create vehicle only for himself
            var resident = await _context.ResidentProfiles
                .FirstOrDefaultAsync(r =>
                    r.ApplicationUserId == user.Id);

            if (resident == null)
                return Forbid();

            var model = new Vehicle
            {
                ResidentProfileId = resident.Id
            };

            return View(model);
        }


        // =========================================================
        // POST: Vehicle/Create
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Vehicle vehicle)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
                return Challenge();


            // -----------------------------------------------------
            // Model validation
            // -----------------------------------------------------

            if (!ModelState.IsValid)
            {
                if (User.IsInRole("Admin"))
                    await LoadResidentsAsync();

                return View(vehicle);
            }


            // -----------------------------------------------------
            // Resident can only assign vehicle to himself
            // -----------------------------------------------------

            if (!User.IsInRole("Admin"))
            {
                var resident = await _context.ResidentProfiles
                    .FirstOrDefaultAsync(r =>
                        r.ApplicationUserId == currentUser.Id);

                if (resident == null)
                    return Forbid();

                vehicle.ResidentProfileId = resident.Id;
            }


            // -----------------------------------------------------
            // Check resident exists
            // -----------------------------------------------------

            var residentCheck = await _context.ResidentProfiles
                .FirstOrDefaultAsync(r =>
                    r.Id == vehicle.ResidentProfileId);

            if (residentCheck == null)
            {
                ModelState.AddModelError(
                    nameof(vehicle.ResidentProfileId),
                    "Selected resident does not exist.");

                if (User.IsInRole("Admin"))
                    await LoadResidentsAsync();

                return View(vehicle);
            }


            // -----------------------------------------------------
            // Check duplicate registration number
            // -----------------------------------------------------

            bool exists = await _context.Vehicles
                .AnyAsync(v =>
                    v.RegistrationNumber.ToLower()
                    == vehicle.RegistrationNumber.ToLower());

            if (exists)
            {
                ModelState.AddModelError(
                    nameof(vehicle.RegistrationNumber),
                    "A vehicle with this registration number already exists.");

                if (User.IsInRole("Admin"))
                    await LoadResidentsAsync();

                return View(vehicle);
            }


            // -----------------------------------------------------
            // Save vehicle
            // -----------------------------------------------------

            _context.Vehicles.Add(vehicle);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Unable to save changes. Try again later.");

                if (User.IsInRole("Admin"))
                    await LoadResidentsAsync();

                return View(vehicle);
            }


            TempData["Success"] =
                "Vehicle created successfully.";


            // -----------------------------------------------------
            // Audit
            // -----------------------------------------------------

            await _auditService.LogAsync(
                currentUser.Id,
                "Create",
                "Vehicle",
                vehicle.Id.ToString(),
                $"Registration:{vehicle.RegistrationNumber}"
            );


            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // GET: Vehicle/Edit/5
        // =========================================================

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var vehicle = await _context.Vehicles
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vehicle == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();


            // -----------------------------------------------------
            // Authorization
            // -----------------------------------------------------

            if (!User.IsInRole("Admin"))
            {
                var resident = await _context.ResidentProfiles
                    .FirstOrDefaultAsync(r =>
                        r.ApplicationUserId == user.Id);

                if (resident == null)
                    return Forbid();

                if (vehicle.ResidentProfileId != resident.Id)
                    return Forbid();
            }


            // -----------------------------------------------------
            // Load residents for Admin
            // -----------------------------------------------------

            if (User.IsInRole("Admin"))
                await LoadResidentsAsync();


            return View(vehicle);
        }


        // =========================================================
        // POST: Vehicle/Edit/5
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Vehicle vehicle)
        {
            if (id != vehicle.Id)
                return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
                return Challenge();


            // -----------------------------------------------------
            // Model validation
            // -----------------------------------------------------

            if (!ModelState.IsValid)
            {
                if (User.IsInRole("Admin"))
                    await LoadResidentsAsync();

                return View(vehicle);
            }


            // -----------------------------------------------------
            // Resident ownership
            // -----------------------------------------------------

            if (!User.IsInRole("Admin"))
            {
                var resident = await _context.ResidentProfiles
                    .FirstOrDefaultAsync(r =>
                        r.ApplicationUserId == currentUser.Id);

                if (resident == null)
                    return Forbid();

                // IMPORTANT:
                // Resident cannot change vehicle ownership
                vehicle.ResidentProfileId = resident.Id;
            }


            // -----------------------------------------------------
            // Check resident exists
            // -----------------------------------------------------

            var residentCheck = await _context.ResidentProfiles
                .FirstOrDefaultAsync(r =>
                    r.Id == vehicle.ResidentProfileId);

            if (residentCheck == null)
            {
                ModelState.AddModelError(
                    nameof(vehicle.ResidentProfileId),
                    "Selected resident does not exist.");

                if (User.IsInRole("Admin"))
                    await LoadResidentsAsync();

                return View(vehicle);
            }


            // -----------------------------------------------------
            // Check duplicate registration
            // -----------------------------------------------------

            bool exists = await _context.Vehicles
                .AnyAsync(v =>
                    v.Id != vehicle.Id &&
                    v.RegistrationNumber.ToLower()
                    == vehicle.RegistrationNumber.ToLower());

            if (exists)
            {
                ModelState.AddModelError(
                    nameof(vehicle.RegistrationNumber),
                    "A vehicle with this registration number already exists.");

                if (User.IsInRole("Admin"))
                    await LoadResidentsAsync();

                return View(vehicle);
            }


            // -----------------------------------------------------
            // Update vehicle
            // -----------------------------------------------------

            try
            {
                _context.Update(vehicle);

                await _context.SaveChangesAsync();


                // Audit
                await _auditService.LogAsync(
                    currentUser.Id,
                    "Update",
                    "Vehicle",
                    vehicle.Id.ToString(),
                    $"Registration:{vehicle.RegistrationNumber}"
                );
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VehicleExists(vehicle.Id))
                    return NotFound();

                throw;
            }


            TempData["Success"] =
                "Vehicle updated successfully.";


            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // GET: Vehicle/Delete/5
        // =========================================================

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var vehicle = await _context.Vehicles
                .Include(v => v.ResidentProfile)
                .ThenInclude(r => r.Flat)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vehicle == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();


            // Admin can delete all
            // Resident can delete only own vehicle
            if (!User.IsInRole("Admin") &&
                vehicle.ResidentProfile.ApplicationUserId != user.Id)
            {
                return Forbid();
            }

            return View(vehicle);
        }


        // =========================================================
        // POST: Vehicle/Delete/5
        // =========================================================

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vehicle = await _context.Vehicles
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vehicle == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();


            // -----------------------------------------------------
            // Authorization
            // -----------------------------------------------------

            if (!User.IsInRole("Admin"))
            {
                var resident = await _context.ResidentProfiles
                    .FirstOrDefaultAsync(r =>
                        r.ApplicationUserId == user.Id);

                if (resident == null)
                    return Forbid();

                if (resident.Id != vehicle.ResidentProfileId)
                    return Forbid();
            }


            // -----------------------------------------------------
            // Store registration before delete
            // -----------------------------------------------------

            var registrationNumber =
                vehicle.RegistrationNumber;


            // -----------------------------------------------------
            // Delete
            // -----------------------------------------------------

            _context.Vehicles.Remove(vehicle);

            await _context.SaveChangesAsync();


            TempData["Success"] =
                "Vehicle deleted successfully.";


            // -----------------------------------------------------
            // Audit
            // -----------------------------------------------------

            await _auditService.LogAsync(
                user.Id,
                "Delete",
                "Vehicle",
                id.ToString(),
                $"Registration:{registrationNumber}"
            );


            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // Vehicle Exists
        // =========================================================

        private bool VehicleExists(int id)
        {
            return _context.Vehicles
                .Any(e => e.Id == id);
        }


        // =========================================================
        // Load Residents
        // =========================================================

        private async Task LoadResidentsAsync()
        {
            var residents = await _context.ResidentProfiles
                .Include(r => r.Flat)
                .ToListAsync();

            var items = residents
                .Select(r => new SelectListItem
                {
                    Value = r.Id.ToString(),
                    Text =
                        $"{r.FullName} " +
                        $"({r.Flat.BlockName}-{r.Flat.FlatNumber})"
                })
                .ToList();

            ViewBag.Residents = items;
        }
    }
}