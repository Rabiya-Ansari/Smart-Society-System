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
    // Admin, Resident and Maintenance Staff can access
    // controller-level actions.
    [Authorize(Roles = "Admin,Resident,MaintenanceStaff")]
    public class MaintenanceController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MaintenanceController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        // =========================================================
        // INDEX
        // =========================================================

        public async Task<IActionResult> Index()
        {
            var query = _context.MaintenanceBills
                .Include(b => b.Flat)
                .AsQueryable();


            // ==========================================
            // RESIDENT
            // Resident can only see bills of own flat
            // ==========================================

            if (User.IsInRole("Resident"))
            {
                var resident = await GetResidentAsync();

                if (resident == null)
                    return Forbid();

                query = query.Where(
                    b => b.FlatId == resident.FlatId
                );
            }


            // ==========================================
            // MAINTENANCE STAFF
            // ==========================================
            // Maintenance Staff can view maintenance bills.
            // No flat restriction is applied here.
            //
            // If later you want Maintenance Staff to see
            // only bills related to assigned complaints,
            // we can add that logic separately.
            // ==========================================


            var bills = await query
                .OrderByDescending(b => b.BillingMonth)
                .ToListAsync();

            return View(bills);
        }


        // =========================================================
        // DETAILS
        // =========================================================

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();


            var bill = await _context.MaintenanceBills
                .Include(b => b.Flat)
                .Include(b => b.BillItems)
                .Include(b => b.Payments)
                .FirstOrDefaultAsync(
                    b => b.Id == id
                );


            if (bill == null)
                return NotFound();


            // ==========================================
            // RESIDENT
            // Only own flat bill
            // ==========================================

            if (User.IsInRole("Resident"))
            {
                var resident = await GetResidentAsync();

                if (resident == null ||
                    bill.FlatId != resident.FlatId)
                {
                    return Forbid();
                }
            }


            // ==========================================
            // MAINTENANCE STAFF
            // Can view details
            // ==========================================

            return View(bill);
        }



        // =========================================================
        // CREATE - GET
        // ADMIN ONLY
        // =========================================================

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            await LoadFlats();


            var model = new MaintenanceBill
            {
                BillingMonth = new DateTime(
                    DateTime.Today.Year,
                    DateTime.Today.Month,
                    1
                ),

                DueDate = DateTime.Today.AddMonths(1),

                PenaltyAmount = 0,

                PaymentStatus = PaymentStatus.Pending
            };


            return View(model);
        }



        // =========================================================
        // CREATE - POST
        // ADMIN ONLY
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(
            MaintenanceBill model)
        {
            if (!ModelState.IsValid)
            {
                await LoadFlats();
                return View(model);
            }


            var flatExists = await _context.Flats
                .AnyAsync(f => f.Id == model.FlatId);


            if (!flatExists)
            {
                ModelState.AddModelError(
                    nameof(model.FlatId),
                    "Selected flat does not exist."
                );

                await LoadFlats();

                return View(model);
            }


            try
            {
                model.BillingMonth = new DateTime(
                    model.BillingMonth.Year,
                    model.BillingMonth.Month,
                    1
                );


                // New bill is always Pending
                model.PaymentStatus =
                    PaymentStatus.Pending;


                _context.MaintenanceBills.Add(model);

                await _context.SaveChangesAsync();


                TempData["Success"] =
                    "Maintenance bill created successfully.";


                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(
                    "",
                    "Maintenance bill could not be created: "
                    + ex.Message
                );

                await LoadFlats();

                return View(model);
            }
        }



        // =========================================================
        // EDIT - GET
        // ADMIN ONLY
        // =========================================================

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();


            var bill = await _context.MaintenanceBills
                .Include(b => b.Flat)
                .FirstOrDefaultAsync(
                    b => b.Id == id
                );


            if (bill == null)
                return NotFound();


            await LoadFlats();

            return View(bill);
        }



        // =========================================================
        // EDIT - POST
        // ADMIN ONLY
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(
            int id,
            MaintenanceBill model)
        {
            if (id != model.Id)
                return NotFound();


            if (!ModelState.IsValid)
            {
                await LoadFlats();
                return View(model);
            }


            var flatExists = await _context.Flats
                .AnyAsync(
                    f => f.Id == model.FlatId
                );


            if (!flatExists)
            {
                ModelState.AddModelError(
                    nameof(model.FlatId),
                    "Selected flat does not exist."
                );

                await LoadFlats();

                return View(model);
            }


            var existingBill =
                await _context.MaintenanceBills
                    .FirstOrDefaultAsync(
                        b => b.Id == id
                    );


            if (existingBill == null)
                return NotFound();


            try
            {
                existingBill.FlatId =
                    model.FlatId;


                existingBill.BillingMonth =
                    new DateTime(
                        model.BillingMonth.Year,
                        model.BillingMonth.Month,
                        1
                    );


                existingBill.AmountDue =
                    model.AmountDue;


                existingBill.DueDate =
                    model.DueDate;


                existingBill.PenaltyAmount =
                    model.PenaltyAmount;


                // Do NOT manually change PaymentStatus.
                // Payment system controls payment status.


                await _context.SaveChangesAsync();


                TempData["Success"] =
                    "Maintenance bill updated successfully.";


                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(
                    "",
                    "Maintenance bill could not be updated: "
                    + ex.Message
                );

                await LoadFlats();

                return View(model);
            }
        }



        // =========================================================
        // DELETE - GET
        // ADMIN ONLY
        // =========================================================

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();


            var bill = await _context.MaintenanceBills
                .Include(b => b.Flat)
                .FirstOrDefaultAsync(
                    b => b.Id == id
                );


            if (bill == null)
                return NotFound();


            return View(bill);
        }



        // =========================================================
        // DELETE - POST
        // ADMIN ONLY
        // =========================================================

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(
            int id)
        {
            var bill = await _context.MaintenanceBills
                .Include(b => b.Payments)
                .FirstOrDefaultAsync(
                    b => b.Id == id
                );


            if (bill == null)
                return NotFound();


            if (bill.Payments.Any())
            {
                TempData["Error"] =
                    "Cannot delete a bill that has payments.";

                return RedirectToAction(nameof(Index));
            }


            _context.MaintenanceBills.Remove(bill);

            await _context.SaveChangesAsync();


            TempData["Success"] =
                "Maintenance bill deleted successfully.";


            return RedirectToAction(nameof(Index));
        }



        // =========================================================
        // GET RESIDENT
        // =========================================================

        private async Task<ResidentProfile?> GetResidentAsync()
        {
            var user =
                await _userManager.GetUserAsync(User);


            if (user == null)
                return null;


            return await _context.ResidentProfiles
                .FirstOrDefaultAsync(
                    r => r.ApplicationUserId == user.Id
                );
        }



        // =========================================================
        // LOAD FLATS
        // =========================================================

        private async Task LoadFlats()
        {
            var flats = await _context.Flats
                .OrderBy(f => f.BlockName)
                .ThenBy(f => f.FlatNumber)
                .ToListAsync();


            ViewBag.Flats = new SelectList(
                flats,
                "Id",
                "FlatNumber"
            );
        }
    }
}