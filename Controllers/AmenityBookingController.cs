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
    [Authorize(Roles = "Admin,Resident")]
    public class AmenityBookingController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuditService _audit;

        public AmenityBookingController(AppDbContext context, UserManager<ApplicationUser> userManager, IAuditService audit)
        {
            _context = context; _userManager = userManager; _audit = audit;
        }

        public async Task<IActionResult> Index()
        {
            var query = _context.AmenityBookings.Include(b => b.Amenity).Include(b => b.ResidentProfile).AsQueryable();
            if (User.IsInRole("Resident"))
            {
                var resident = await GetResidentAsync(); if (resident == null) return Forbid();
                query = query.Where(b => b.ResidentProfileId == resident.Id);
            }
            return View(await query.OrderByDescending(b => b.BookingDate).ThenBy(b => b.StartTime).ToListAsync());
        }

        public async Task<IActionResult> Create()
        {
            await LoadLookupsAsync();
            if (User.IsInRole("Resident"))
            {
                var resident = await GetResidentAsync(); if (resident == null) return Forbid();
                ViewBag.Resident = resident;
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AmenityBooking model)
        {
            if (User.IsInRole("Resident"))
            {
                var resident = await GetResidentAsync(); if (resident == null) return Forbid();
                model.ResidentProfileId = resident.Id;
                model.Status = BookingStatus.Pending;
            }

            if (model.EndTime <= model.StartTime)
                ModelState.AddModelError(nameof(model.EndTime), "End time must be after start time.");
            if (model.BookingDate.Date < DateTime.Today)
                ModelState.AddModelError(nameof(model.BookingDate), "Booking date cannot be in the past.");

            var amenity = await _context.Amenities.FirstOrDefaultAsync(a => a.Id == model.AmenityId && a.IsActive);
            if (amenity == null) ModelState.AddModelError(nameof(model.AmenityId), "Selected amenity is not available.");

            if (model.StartTime < TimeSpan.Zero || model.EndTime > TimeSpan.FromDays(1))
                ModelState.AddModelError(string.Empty, "Invalid booking time.");

            if (ModelState.IsValid)
            {
                var conflict = await _context.AmenityBookings.AnyAsync(b => b.AmenityId == model.AmenityId && b.BookingDate.Date == model.BookingDate.Date && b.Status != BookingStatus.Cancelled && model.StartTime < b.EndTime && model.EndTime > b.StartTime);
                if (conflict) ModelState.AddModelError(string.Empty, "This amenity is already booked for the selected time.");
            }

            if (!ModelState.IsValid)
            {
                await LoadLookupsAsync(); return View(model);
            }

            model.CreatedAt = DateTime.UtcNow;
            _context.AmenityBookings.Add(model);
            await _context.SaveChangesAsync();
            var user = await _userManager.GetUserAsync(User);
            if (user != null) await _audit.LogAsync(user.Id, "Create", "AmenityBooking", model.Id.ToString(), $"Amenity:{model.AmenityId};Date:{model.BookingDate:yyyy-MM-dd}");
            TempData["Success"] = "Amenity booking created.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var booking = await _context.AmenityBookings.Include(b => b.Amenity).Include(b => b.ResidentProfile).FirstOrDefaultAsync(b => b.Id == id);
            if (booking == null) return NotFound();
            if (!await CanAccessAsync(booking)) return Forbid();
            return View(booking);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var booking = await _context.AmenityBookings.Include(b => b.Amenity).Include(b => b.ResidentProfile).FirstOrDefaultAsync(b => b.Id == id);
            if (booking == null) return NotFound();
            if (!await CanAccessAsync(booking)) return Forbid();
            return View(booking);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.AmenityBookings.FindAsync(id);
            if (booking == null) return NotFound();
            if (!await CanAccessAsync(booking)) return Forbid();
            booking.Status = BookingStatus.Cancelled;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Booking cancelled.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<ResidentProfile?> GetResidentAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            return user == null ? null : await _context.ResidentProfiles.FirstOrDefaultAsync(r => r.ApplicationUserId == user.Id);
        }
        private async Task<bool> CanAccessAsync(AmenityBooking booking)
        {
            if (User.IsInRole("Admin")) return true;
            var resident = await GetResidentAsync();
            return resident != null && booking.ResidentProfileId == resident.Id;
        }
        private async Task LoadLookupsAsync()
        {
            ViewBag.Amenities = new SelectList(await _context.Amenities.Where(a => a.IsActive).OrderBy(a => a.Name).ToListAsync(), "Id", "Name");
            if (User.IsInRole("Admin")) ViewBag.Residents = new SelectList(await _context.ResidentProfiles.OrderBy(r => r.FullName).ToListAsync(), "Id", "FullName");
            ViewBag.Statuses = new SelectList(Enum.GetValues<BookingStatus>());
        }
    }
}
