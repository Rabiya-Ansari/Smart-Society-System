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

        public EmergencyContactController(AppDbContext context, UserManager<ApplicationUser> userManager, IAuditService auditService)
        {
            _context = context;
            _userManager = userManager;
            _auditService = auditService;
        }

        // GET: EmergencyContact
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Admin"))
            {
                var contacts = await _context.EmergencyContacts
                    .Include(e => e.ResidentProfile)
                    .ThenInclude(r => r.Flat)
                    .ToListAsync();

                return View(contacts);
            }

            var user = await _userManager.GetUserAsync(User);
            var resident = await _context.ResidentProfiles.FirstOrDefaultAsync(r => r.ApplicationUserId == user.Id);
            if (resident == null) return Forbid();

            var mine = await _context.EmergencyContacts
                .Where(e => e.ResidentProfileId == resident.Id)
                .Include(e => e.ResidentProfile)
                .ThenInclude(r => r.Flat)
                .ToListAsync();

            return View(mine);
        }

        // GET: EmergencyContact/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var contact = await _context.EmergencyContacts
                .Include(e => e.ResidentProfile)
                .ThenInclude(r => r.Flat)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (contact == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (!User.IsInRole("Admin") && contact.ResidentProfile.ApplicationUserId != user.Id)
                return Forbid();

            return View(contact);
        }

        // GET: EmergencyContact/Create
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

            var model = new EmergencyContact { ResidentProfileId = resident.Id };
            return View(model);
        }

        // POST: EmergencyContact/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmergencyContact contact)
        {
            if (!ModelState.IsValid)
            {
                if (User.IsInRole("Admin")) await LoadResidentsAsync();
                return View(contact);
            }
            var currentUser = await _userManager.GetUserAsync(User);

            if (!User.IsInRole("Admin"))
            {
                var resident = await _context.ResidentProfiles.FirstOrDefaultAsync(r => r.ApplicationUserId == currentUser.Id);
                if (resident == null) return Forbid();
                contact.ResidentProfileId = resident.Id;
            }

            var residentExists = await _context.ResidentProfiles.AnyAsync(r => r.Id == contact.ResidentProfileId);
            if (!residentExists)
            {
                ModelState.AddModelError(nameof(contact.ResidentProfileId), "Selected resident does not exist.");
                if (User.IsInRole("Admin")) await LoadResidentsAsync();
                return View(contact);
            }

            _context.EmergencyContacts.Add(contact);

            try
            {
                await _context.SaveChangesAsync();

                await _auditService.LogAsync(currentUser.Id, "Create", "EmergencyContact", contact.Id.ToString(), $"Name:{contact.Name}");
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "Unable to save changes. Try again later.");
                if (User.IsInRole("Admin")) await LoadResidentsAsync();
                return View(contact);
            }

            TempData["Success"] = "Emergency contact created successfully.";

            return RedirectToAction(nameof(Index));
        }

        // GET: EmergencyContact/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var contact = await _context.EmergencyContacts
                .FirstOrDefaultAsync(e => e.Id == id);

            if (contact == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (!User.IsInRole("Admin") && contact.ResidentProfileId != (await _context.ResidentProfiles.FirstOrDefaultAsync(r => r.ApplicationUserId == user.Id))?.Id)
                return Forbid();

            if (User.IsInRole("Admin")) await LoadResidentsAsync();

            return View(contact);
        }

        // POST: EmergencyContact/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EmergencyContact contact)
        {
            if (id != contact.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                if (User.IsInRole("Admin")) await LoadResidentsAsync();
                return View(contact);
            }
            var currentUser = await _userManager.GetUserAsync(User);

            if (!User.IsInRole("Admin"))
            {
                var resident = await _context.ResidentProfiles.FirstOrDefaultAsync(r => r.ApplicationUserId == currentUser.Id);
                if (resident == null) return Forbid();
                contact.ResidentProfileId = resident.Id;
            }

            var residentExists = await _context.ResidentProfiles.AnyAsync(r => r.Id == contact.ResidentProfileId);
            if (!residentExists)
            {
                ModelState.AddModelError(nameof(contact.ResidentProfileId), "Selected resident does not exist.");
                if (User.IsInRole("Admin")) await LoadResidentsAsync();
                return View(contact);
            }

            try
            {
                _context.Update(contact);
                await _context.SaveChangesAsync();

                await _auditService.LogAsync(currentUser.Id, "Update", "EmergencyContact", contact.Id.ToString(), $"Name:{contact.Name}");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmergencyContactExists(contact.Id)) return NotFound();
                throw;
            }

            TempData["Success"] = "Emergency contact updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        // GET: EmergencyContact/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var contact = await _context.EmergencyContacts
                .Include(e => e.ResidentProfile)
                .ThenInclude(r => r.Flat)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (contact == null) return NotFound();

            return View(contact);
        }

        // POST: EmergencyContact/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contact = await _context.EmergencyContacts.FindAsync(id);

            if (contact == null) return NotFound();
            var user = await _userManager.GetUserAsync(User);
            if (!User.IsInRole("Admin") && contact.ResidentProfileId != (await _context.ResidentProfiles.FirstOrDefaultAsync(r => r.ApplicationUserId == user.Id))?.Id)
                return Forbid();

            _context.EmergencyContacts.Remove(contact);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(user.Id, "Delete", "EmergencyContact", contact.Id.ToString(), $"Name:{contact.Name}");

            TempData["Success"] = "Emergency contact deleted successfully.";

            return RedirectToAction(nameof(Index));
        }

        private bool EmergencyContactExists(int id)
            => _context.EmergencyContacts.Any(e => e.Id == id);

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
