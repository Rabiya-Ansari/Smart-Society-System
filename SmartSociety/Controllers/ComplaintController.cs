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
    [Authorize(Roles = "Admin,Resident,MaintenanceStaff")]
    public class ComplaintController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ComplaintController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        // ============================================================
        // INDEX
        // ============================================================

        public async Task<IActionResult> Index()
        {
            var query = _context.Complaints
                .Include(c => c.ResidentProfile)
                    .ThenInclude(r => r.ApplicationUser)
                .Include(c => c.AssignedStaff)
                .AsQueryable();


            // ========================================================
            // MAINTENANCE STAFF
            // Only assigned complaints
            // ========================================================

            if (User.IsInRole("MaintenanceStaff"))
            {
                var currentUser = await _userManager.GetUserAsync(User);

                if (currentUser == null)
                    return Forbid();

                query = query.Where(
                    c => c.AssignedStaffId == currentUser.Id
                );
            }


            // ========================================================
            // RESIDENT
            // Only own complaints
            // ========================================================

            if (User.IsInRole("Resident"))
            {
                var currentUser = await _userManager.GetUserAsync(User);

                if (currentUser == null)
                    return Forbid();

                var resident = await _context.ResidentProfiles
                    .FirstOrDefaultAsync(
                        r => r.ApplicationUserId == currentUser.Id
                    );

                if (resident == null)
                    return Forbid();

                query = query.Where(
                    c => c.ResidentProfileId == resident.Id
                );
            }


            var complaints = await query
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();


            return View(complaints);
        }


        // ============================================================
        // DETAILS
        // ============================================================

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();


            var complaint = await _context.Complaints
                .Include(c => c.ResidentProfile)
                    .ThenInclude(r => r.ApplicationUser)
                .Include(c => c.AssignedStaff)
                .FirstOrDefaultAsync(c => c.Id == id);


            if (complaint == null)
                return NotFound();


            // ========================================================
            // MAINTENANCE STAFF
            // Can only see assigned complaint
            // ========================================================

            if (User.IsInRole("MaintenanceStaff"))
            {
                var currentUser = await _userManager.GetUserAsync(User);

                if (currentUser == null ||
                    complaint.AssignedStaffId != currentUser.Id)
                {
                    return Forbid();
                }
            }


            // ========================================================
            // RESIDENT
            // Can only see own complaint
            // ========================================================

            if (User.IsInRole("Resident"))
            {
                var currentUser = await _userManager.GetUserAsync(User);

                if (currentUser == null)
                    return Forbid();


                var resident = await _context.ResidentProfiles
                    .FirstOrDefaultAsync(
                        r => r.ApplicationUserId == currentUser.Id
                    );


                if (resident == null ||
                    complaint.ResidentProfileId != resident.Id)
                {
                    return Forbid();
                }
            }


            return View(complaint);
        }


        // ============================================================
        // CREATE - GET
        // Admin only
        // ============================================================

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            await LoadResidents();
            await LoadMaintenanceStaff();

            var model = new Complaint
            {
                Status = ComplaintStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            return View(model);
        }


        // ============================================================
        // CREATE - POST
        // Admin only
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Complaint model)
        {
            if (!ModelState.IsValid)
            {
                await LoadResidents();
                await LoadMaintenanceStaff();
                return View(model);
            }


            // Make sure resident exists

            var residentExists = await _context.ResidentProfiles
                .AnyAsync(r => r.Id == model.ResidentProfileId);


            if (!residentExists)
            {
                ModelState.AddModelError(
                    nameof(model.ResidentProfileId),
                    "Selected resident does not exist."
                );

                await LoadResidents();
                await LoadMaintenanceStaff();

                return View(model);
            }


            // Validate assigned maintenance staff

            if (!string.IsNullOrWhiteSpace(model.AssignedStaffId))
            {
                var staff = await _userManager.FindByIdAsync(
                    model.AssignedStaffId
                );

                if (staff == null ||
                    !await _userManager.IsInRoleAsync(
                        staff,
                        "MaintenanceStaff"))
                {
                    ModelState.AddModelError(
                        nameof(model.AssignedStaffId),
                        "Selected maintenance staff is invalid."
                    );

                    await LoadResidents();
                    await LoadMaintenanceStaff();

                    return View(model);
                }
            }


            model.CreatedAt = DateTime.UtcNow;

            if (model.Status == ComplaintStatus.Resolved)
            {
                model.ResolvedAt = DateTime.UtcNow;
            }


            _context.Complaints.Add(model);

            await _context.SaveChangesAsync();


            TempData["Success"] =
                "Complaint created successfully.";


            return RedirectToAction(nameof(Index));
        }


        // ============================================================
        // EDIT - GET
        // Admin + Maintenance Staff
        // ============================================================

        [Authorize(Roles = "Admin,MaintenanceStaff")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();


            var complaint = await _context.Complaints
                .Include(c => c.ResidentProfile)
                .Include(c => c.AssignedStaff)
                .FirstOrDefaultAsync(c => c.Id == id);


            if (complaint == null)
                return NotFound();


            // ========================================================
            // MAINTENANCE STAFF
            // Only assigned complaint can be edited
            // ========================================================

            if (User.IsInRole("MaintenanceStaff"))
            {
                var currentUser = await _userManager.GetUserAsync(User);

                if (currentUser == null ||
                    complaint.AssignedStaffId != currentUser.Id)
                {
                    return Forbid();
                }
            }


            await LoadResidents();
            await LoadMaintenanceStaff();


            return View(complaint);
        }


        // ============================================================
        // EDIT - POST
        // Admin + Maintenance Staff
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,MaintenanceStaff")]
        public async Task<IActionResult> Edit(
            int id,
            Complaint model)
        {
            if (id != model.Id)
                return NotFound();


            var existingComplaint =
                await _context.Complaints
                    .FirstOrDefaultAsync(c => c.Id == id);


            if (existingComplaint == null)
                return NotFound();


            // ========================================================
            // MAINTENANCE STAFF
            // Only assigned complaint
            // ========================================================

            if (User.IsInRole("MaintenanceStaff"))
            {
                var currentUser =
                    await _userManager.GetUserAsync(User);


                if (currentUser == null ||
                    existingComplaint.AssignedStaffId != currentUser.Id)
                {
                    return Forbid();
                }


                // ====================================================
                // MAINTENANCE STAFF CAN ONLY CHANGE:
                //
                // Status
                // Work Notes
                // ====================================================

                existingComplaint.Status = model.Status;
                existingComplaint.WorkNotes = model.WorkNotes;


                // ====================================================
                // RESOLVED DATE
                // ====================================================

                if (model.Status == ComplaintStatus.Resolved)
                {
                    if (existingComplaint.ResolvedAt == null)
                    {
                        existingComplaint.ResolvedAt =
                            DateTime.UtcNow;
                    }
                }
                else
                {
                    existingComplaint.ResolvedAt = null;
                }


                await _context.SaveChangesAsync();


                TempData["Success"] =
                    "Complaint status and work notes updated successfully.";


                return RedirectToAction(
                    nameof(Details),
                    new { id = existingComplaint.Id }
                );
            }


            // ========================================================
            // ADMIN
            // Admin can update complete complaint
            // ========================================================

            if (!ModelState.IsValid)
            {
                await LoadResidents();
                await LoadMaintenanceStaff();

                return View(model);
            }


            existingComplaint.ResidentProfileId =
                model.ResidentProfileId;

            existingComplaint.Category =
                model.Category;

            existingComplaint.Description =
                model.Description;

            existingComplaint.AssignedStaffId =
                model.AssignedStaffId;

            existingComplaint.WorkNotes =
                model.WorkNotes;

            existingComplaint.SlaTargetDate =
                model.SlaTargetDate;

            existingComplaint.Status =
                model.Status;


            if (model.Status == ComplaintStatus.Resolved)
            {
                if (existingComplaint.ResolvedAt == null)
                {
                    existingComplaint.ResolvedAt =
                        DateTime.UtcNow;
                }
            }
            else
            {
                existingComplaint.ResolvedAt = null;
            }


            await _context.SaveChangesAsync();


            TempData["Success"] =
                "Complaint updated successfully.";


            return RedirectToAction(nameof(Index));
        }


        // ============================================================
        // DELETE - GET
        // Admin only
        // ============================================================

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();


            var complaint = await _context.Complaints
                .Include(c => c.ResidentProfile)
                    .ThenInclude(r => r.ApplicationUser)
                .Include(c => c.AssignedStaff)
                .FirstOrDefaultAsync(c => c.Id == id);


            if (complaint == null)
                return NotFound();


            return View(complaint);
        }


        // ============================================================
        // DELETE - POST
        // Admin only
        // ============================================================

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var complaint =
                await _context.Complaints
                    .FirstOrDefaultAsync(c => c.Id == id);


            if (complaint == null)
                return NotFound();


            _context.Complaints.Remove(complaint);

            await _context.SaveChangesAsync();


            TempData["Success"] =
                "Complaint deleted successfully.";


            return RedirectToAction(nameof(Index));
        }


        // ============================================================
        // LOAD RESIDENTS
        // ============================================================

        private async Task LoadResidents()
        {
            var residents =
                await _context.ResidentProfiles
                    .Include(r => r.ApplicationUser)
                    .OrderBy(r => r.FullName)
                    .ToListAsync();


            ViewBag.Residents =
                new SelectList(
                    residents,
                    "Id",
                    "FullName"
                );
        }


        // ============================================================
        // LOAD MAINTENANCE STAFF
        // ============================================================

        private async Task LoadMaintenanceStaff()
        {
            var staff =
                await _userManager.GetUsersInRoleAsync(
                    "MaintenanceStaff"
                );


            ViewBag.MaintenanceStaff =
                new SelectList(
                    staff,
                    "Id",
                    "Email"
                );
        }
    }
}