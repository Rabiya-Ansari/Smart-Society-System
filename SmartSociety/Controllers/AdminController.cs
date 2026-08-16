using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartSociety.Data;

namespace SmartSociety.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var totalFlats = await _context.Flats.CountAsync();
            var occupiedFlats = await _context.Flats.CountAsync(f => f.IsOccupied);
            var availableFlats = totalFlats - occupiedFlats;
            var totalResidents = await _context.ResidentProfiles.CountAsync();
            var totalVehicles = await _context.Vehicles.CountAsync();
            var pendingComplaints = await _context.Complaints.CountAsync(c => c.Status == Models.Enums.ComplaintStatus.Pending || c.Status == Models.Enums.ComplaintStatus.InProgress);
            var unpaidBills = await _context.MaintenanceBills.CountAsync(b => b.PaymentStatus != Models.Enums.PaymentStatus.Paid);
            var totalAmenities = await _context.Amenities.CountAsync();
            var upcomingBookings = await _context.AmenityBookings.CountAsync(b => b.BookingDate >= DateTime.UtcNow.Date);
            var activeNotices = await _context.Notices.CountAsync(n => n.IsPublished && n.PublishDate <= DateTime.UtcNow && n.ExpiryDate > DateTime.UtcNow);
            var activePolls = await _context.Polls.CountAsync(p => p.IsActive && p.StartDate <= DateTime.UtcNow && p.EndDate >= DateTime.UtcNow);

            ViewBag.TotalFlats = totalFlats;
            ViewBag.OccupiedFlats = occupiedFlats;
            ViewBag.AvailableFlats = availableFlats;
            ViewBag.TotalResidents = totalResidents;
            ViewBag.TotalVehicles = totalVehicles;
            ViewBag.PendingComplaints = pendingComplaints;
            ViewBag.UnpaidBills = unpaidBills;
            ViewBag.TotalAmenities = totalAmenities;
            ViewBag.UpcomingBookings = upcomingBookings;
            ViewBag.ActiveNotices = activeNotices;
            ViewBag.ActivePolls = activePolls;

            return View();
        }
    }
}
