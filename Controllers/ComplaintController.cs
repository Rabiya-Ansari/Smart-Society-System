using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartSociety.Data;
using SmartSociety.Models;
using SmartSociety.Models.Enums;
using SmartSociety.Services;

namespace SmartSociety.Controllers
{
    [Authorize(Roles = "Admin,Resident,MaintenanceStaff")]
    public class ComplaintController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuditService _audit;

        public ComplaintController(AppDbContext context, UserManager<ApplicationUser> userManager, IAuditService audit)
        {
            _context = context;
            _userManager = userManager;
            _audit = audit;
        }

        public async Task<IActionResult> Index()
        {
            IQueryable<Complaint> query = _context.Complaints
                .Include(c => c.ResidentProfile)
                .Include(c => c.AssignedStaff);

            if (User.IsInRole("Resident"))
            {
                var resident = await GetResidentAsync();
                if (resident == null) return Forbid();
                query = query.Where(c => c.ResidentProfileId == resident.Id);
            }
            else if (User.IsInRole("MaintenanceStaff"))
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Challenge();
                query = query.Where(c => c.AssignedStaffId == user.Id);
            }

            return View(await query.OrderByDescending(c => c.CreatedAt).ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var complaint = await _context.Complaints.Include(c => c.ResidentProfile).Include(c => c.AssignedStaff).FirstOrDefaultAsync(c => c.Id == id);
            if (complaint == null) return NotFound();
            if (!await CanAccessAsync(complaint)) return Forbid();
            return View(complaint);
        }

        public async Task<IActionResult> Create()
        {
            if (User.IsInRole("MaintenanceStaff")) return Forbid();
            if (User.IsInRole("Admin")) await LoadResidentsAsync();
            ViewBag.Categories = new SelectList(Enum.GetValues<ComplaintCategory>());
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Complaint model)
        {
            if (User.IsInRole("MaintenanceStaff")) return Forbid();
            if (User.IsInRole("Resident"))
            {
                var resident = await GetResidentAsync();
                if (resident == null) return Forbid();
                model.ResidentProfileId = resident.Id;
            }

            if (!ModelState.IsValid)
            {
                if (User.IsInRole("Admin")) await LoadResidentsAsync();
                ViewBag.Categories = new SelectList(Enum.GetValues<ComplaintCategory>(), model.Category);
                return View(model);
            }

            if (!await _context.ResidentProfiles.AnyAsync(r => r.Id == model.ResidentProfileId))
            {
                ModelState.AddModelError(nameof(model.ResidentProfileId), "Selected resident does not exist.");
                if (User.IsInRole("Admin")) await LoadResidentsAsync();
                ViewBag.Categories = new SelectList(Enum.GetValues<ComplaintCategory>(), model.Category);
                return View(model);
            }

            model.Status = ComplaintStatus.Pending;
            model.CreatedAt = DateTime.UtcNow;
            model.AssignedStaffId = null;
            model.ResolvedAt = null;
            _context.Complaints.Add(model);
            await _context.SaveChangesAsync();
            var user = await _userManager.GetUserAsync(User);
            if (user != null) await _audit.LogAsync(user.Id, "Create", "Complaint", model.Id.ToString(), $"Category:{model.Category}");
            TempData["Success"] = "Complaint submitted successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var complaint = await _context.Complaints.FindAsync(id);
            if (complaint == null) return NotFound();
            if (!await CanAccessAsync(complaint)) return Forbid();

            if (User.IsInRole("MaintenanceStaff"))
            {
                if (complaint.Status == ComplaintStatus.Resolved) return Forbid();
                ViewBag.Categories = new SelectList(Enum.GetValues<ComplaintCategory>(), complaint.Category);
                ViewBag.Statuses = new SelectList(new[] { ComplaintStatus.InProgress, ComplaintStatus.Resolved }, complaint.Status);
                return View(complaint);
            }

            if (User.IsInRole("Resident") && complaint.Status != ComplaintStatus.Pending)
                return Forbid();

            if (User.IsInRole("Admin"))
            {
                await LoadResidentsAsync();
                await LoadStaffAsync();
            }
            ViewBag.Categories = new SelectList(Enum.GetValues<ComplaintCategory>(), complaint.Category);
            ViewBag.Statuses = new SelectList(Enum.GetValues<ComplaintStatus>(), complaint.Status);
            return View(complaint);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Complaint model)
        {
            if (id != model.Id) return NotFound();
            var complaint = await _context.Complaints.FindAsync(id);
            if (complaint == null) return NotFound();
            if (!await CanAccessAsync(complaint)) return Forbid();

            if (User.IsInRole("MaintenanceStaff"))
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.Categories = new SelectList(Enum.GetValues<ComplaintCategory>(), model.Category);
                    ViewBag.Statuses = new SelectList(new[] { ComplaintStatus.InProgress, ComplaintStatus.Resolved }, model.Status);
                    return View(model);
                }
                if (model.Status is not (ComplaintStatus.InProgress or ComplaintStatus.Resolved))
                {
                    ModelState.AddModelError(nameof(model.Status), "Maintenance staff can set only InProgress or Resolved.");
                    return View(model);
                }
                complaint.Status = model.Status;
                complaint.WorkNotes = model.WorkNotes;
                if (model.Status == ComplaintStatus.Resolved) complaint.ResolvedAt = DateTime.UtcNow;
                else complaint.ResolvedAt = null;
            }
            else if (User.IsInRole("Resident"))
            {
                if (complaint.Status != ComplaintStatus.Pending) return Forbid();
                if (!ModelState.IsValid) { ViewBag.Categories = new SelectList(Enum.GetValues<ComplaintCategory>(), model.Category); return View(model); }
                complaint.Category = model.Category;
                complaint.Description = model.Description;
            }
            else
            {
                if (!ModelState.IsValid)
                {
                    await LoadResidentsAsync(); await LoadStaffAsync();
                    ViewBag.Categories = new SelectList(Enum.GetValues<ComplaintCategory>(), model.Category);
                    ViewBag.Statuses = new SelectList(Enum.GetValues<ComplaintStatus>(), model.Status);
                    return View(model);
                }
                complaint.ResidentProfileId = model.ResidentProfileId;
                complaint.Category = model.Category;
                complaint.Description = model.Description;
                complaint.Status = model.Status;
                complaint.AssignedStaffId = model.AssignedStaffId;
                complaint.WorkNotes = model.WorkNotes;
                complaint.SlaTargetDate = model.SlaTargetDate;
                complaint.ResolvedAt = model.Status == ComplaintStatus.Resolved ? DateTime.UtcNow : null;
            }

            await _context.SaveChangesAsync();
            var user = await _userManager.GetUserAsync(User);
            if (user != null) await _audit.LogAsync(user.Id, "Update", "Complaint", id.ToString(), $"Status:{complaint.Status}");
            TempData["Success"] = "Complaint updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            if (!User.IsInRole("Admin")) return Forbid();
            var complaint = await _context.Complaints.Include(c => c.ResidentProfile).FirstOrDefaultAsync(c => c.Id == id);
            return complaint == null ? NotFound() : View(complaint);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!User.IsInRole("Admin")) return Forbid();
            var complaint = await _context.Complaints.FindAsync(id);
            if (complaint == null) return NotFound();
            _context.Complaints.Remove(complaint);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Complaint deleted.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<ResidentProfile?> GetResidentAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            return user == null ? null : await _context.ResidentProfiles.FirstOrDefaultAsync(r => r.ApplicationUserId == user.Id);
        }

        private async Task<bool> CanAccessAsync(Complaint c)
        {
            if (User.IsInRole("Admin")) return true;
            if (User.IsInRole("MaintenanceStaff"))
            {
                var user = await _userManager.GetUserAsync(User);
                return user != null && c.AssignedStaffId == user.Id;
            }
            var resident = await GetResidentAsync();
            return resident != null && c.ResidentProfileId == resident.Id;
        }

        private async Task LoadResidentsAsync()
        {
            var residents = await _context.ResidentProfiles.Include(r => r.Flat).OrderBy(r => r.FullName).ToListAsync();
            ViewBag.Residents = residents.Select(r => new SelectListItem { Value = r.Id.ToString(), Text = $"{r.FullName} ({r.Flat.BlockName}-{r.Flat.FlatNumber})" }).ToList();
        }

        private async Task LoadStaffAsync()
        {
            var staff = await _userManager.GetUsersInRoleAsync("MaintenanceStaff");
            ViewBag.Staff = staff.Select(s => new SelectListItem { Value = s.Id, Text = s.Email ?? s.UserName ?? s.Id }).ToList();
        }
    }
}
