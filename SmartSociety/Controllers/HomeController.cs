using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartSociety.Data;
using SmartSociety.Models;
using System.Diagnostics;

namespace SmartSociety.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _context;

        public HomeController(
            UserManager<ApplicationUser> userManager,
            AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // ==========================================
            // NOT LOGGED IN
            // Show public Hero / Landing Page
            // ==========================================

            if (!(User.Identity?.IsAuthenticated ?? false))
            {
                return View();
            }


            // ==========================================
            // ADMIN
            // ==========================================

            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Admin");
            }


            // ==========================================
            // SECURITY STAFF
            // ==========================================

            if (User.IsInRole("SecurityStaff"))
            {
                return RedirectToAction("Index", "Security");
            }


            // ==========================================
            // MAINTENANCE STAFF
            // ==========================================

            if (User.IsInRole("MaintenanceStaff"))
            {
                return RedirectToAction("Index", "Complaint");
            }


            // ==========================================
            // RESIDENT
            // ==========================================

            if (User.IsInRole("Resident"))
            {
                var user = await _userManager.GetUserAsync(User);

                var resident = user == null
                    ? null
                    : await _context.ResidentProfiles
                        .Include(r => r.Flat)
                        .FirstOrDefaultAsync(
                            r => r.ApplicationUserId == user.Id
                        );

                if (resident == null)
                {
                    return RedirectToPage(
                        "/Account/AccessDenied",
                        new { area = "Identity" }
                    );
                }


                ViewBag.Resident = resident;


                ViewBag.VehicleCount =
                    await _context.Vehicles.CountAsync(
                        v =>
                            v.ResidentProfileId == resident.Id &&
                            v.IsActive
                    );


                ViewBag.VisitorCount =
                    await _context.Visitors.CountAsync(
                        v =>
                            v.FlatId == resident.FlatId &&
                            v.ValidUntil >= DateTime.Now
                    );


                ViewBag.ComplaintCount =
                    await _context.Complaints.CountAsync(
                        c =>
                            c.ResidentProfileId == resident.Id &&
                            c.Status != Models.Enums.ComplaintStatus.Resolved
                    );


                ViewBag.BookingCount =
                    await _context.AmenityBookings.CountAsync(
                        b =>
                            b.ResidentProfileId == resident.Id &&
                            b.BookingDate >= DateTime.Today
                    );


                ViewBag.PendingBills =
                    await _context.MaintenanceBills.CountAsync(
                        b =>
                            b.FlatId == resident.FlatId &&
                            b.PaymentStatus != Models.Enums.PaymentStatus.Paid
                    );


                ViewBag.NoticeCount =
                    await _context.Notices.CountAsync(
                        n =>
                            n.IsPublished &&
                            n.PublishDate <= DateTime.Now &&
                            n.ExpiryDate > DateTime.Now
                    );


                return View("ResidentDashboard");
            }


            // ==========================================
            // UNKNOWN ROLE
            // ==========================================

            return RedirectToPage(
                "/Account/AccessDenied",
                new { area = "Identity" }
            );
        }


        public IActionResult Sitemap()
        {
            return View();
        }


        public IActionResult Privacy()
        {
            return View();
        }


        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true
        )]
        public IActionResult Error()
        {
            return View(
                new ErrorViewModel
                {
                    RequestId =
                        Activity.Current?.Id ??
                        HttpContext.TraceIdentifier
                }
            );
        }
    }
}