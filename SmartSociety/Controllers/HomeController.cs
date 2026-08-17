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
            // NOT LOGGED IN
            // Show public Hero / Landing Page

            if (!(User.Identity?.IsAuthenticated ?? false))
            {
                return View();
            }

            // ADMIN
           
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Admin");
            }
             // SECURITY STAFF
           
            if (User.IsInRole("SecurityStaff"))
            {
                ViewBag.VisitorCount =
                    await _context.Visitors.CountAsync(
                        v => v.ValidUntil >= DateTime.Now
                    );

                ViewBag.TodayEntries =
                    await _context.GateLogs.CountAsync(
                        g => g.EntryTime.Date == DateTime.Today
                    );

                ViewBag.TodayExits =
                    await _context.GateLogs.CountAsync(
                        g => g.ExitTime.HasValue &&
                             g.ExitTime.Value.Date == DateTime.Today
                    );

                ViewBag.ActiveInside =
                    await _context.GateLogs.CountAsync(
                        g => !g.ExitTime.HasValue
                    );

                return View("SecurityDashboard");
            }

            // MAINTENANCE STAFF
            
            if (User.IsInRole("MaintenanceStaff"))
            {
                ViewBag.OpenComplaints =
                    await _context.Complaints.CountAsync(
                        c => c.Status != Models.Enums.ComplaintStatus.Resolved
                    );

                ViewBag.AssignedComplaints =
                    await _context.Complaints.CountAsync(
                        c => c.AssignedStaffId != null
                    );

                ViewBag.TotalComplaints =
                    await _context.Complaints.CountAsync();

                ViewBag.PendingComplaints =
                    await _context.Complaints.CountAsync(
                        c => c.Status == Models.Enums.ComplaintStatus.Pending
                    );

                return View("MaintenanceDashboard");
            }
            // RESIDENT
           
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


            // UNKNOWN ROLE
           

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