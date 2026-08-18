using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using SmartSociety.Data;
using SmartSociety.Models;
using SmartSociety.Services;
using System.Security.Claims;

namespace SmartSociety.Controllers
{
    [Authorize(Roles = "Admin,Resident,SecurityStaff")]
    public class VisitorController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IAuditService _auditService;

        public VisitorController(
            AppDbContext context,
            IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }


        // =========================================================
        // GET: Visitor
        // =========================================================

        public async Task<IActionResult> Index()
        {
            // Admin and SecurityStaff can see all visitors
            if (User.IsInRole("Admin") ||
                User.IsInRole("SecurityStaff"))
            {
                var allVisitors = await _context.Visitors
                    .Include(v => v.Flat)
                    .OrderByDescending(v => v.ValidFrom)
                    .ToListAsync();

                return View(allVisitors);
            }

            // Resident can see only visitors belonging to own flat
            var resident = await GetCurrentResidentAsync();

            if (resident == null)
                return Forbid();

            var visitors = await _context.Visitors
                .Where(v => v.FlatId == resident.FlatId)
                .Include(v => v.Flat)
                .OrderByDescending(v => v.ValidFrom)
                .ToListAsync();

            return View(visitors);
        }


        // =========================================================
        // GET: Visitor/Details/5
        // =========================================================

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var visitor = await _context.Visitors
                .Include(v => v.Flat)
                .Include(v => v.GateLogs)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (visitor == null)
                return NotFound();

            // Admin/Security can view all
            if (User.IsInRole("Admin") ||
                User.IsInRole("SecurityStaff"))
            {
                return View(visitor);
            }

            // Resident can view only own flat visitor
            var resident = await GetCurrentResidentAsync();

            if (resident == null)
                return Forbid();

            if (visitor.FlatId != resident.FlatId)
                return Forbid();

            return View(visitor);
        }

        // =========================================================
        // GET: Visitor/Pass/5
        // =========================================================

        public async Task<IActionResult> Pass(int? id)
        {
            if (id == null)
                return NotFound();

            var visitor = await _context.Visitors
                .Include(v => v.Flat)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (visitor == null)
                return NotFound();

            // Admin/Security can view all visitor passes
            if (User.IsInRole("Admin") ||
                User.IsInRole("SecurityStaff"))
            {
                // Continue
            }
            else
            {
                // Resident can view only own flat visitor pass
                var resident = await GetCurrentResidentAsync();

                if (resident == null)
                    return Forbid();

                if (visitor.FlatId != resident.FlatId)
                    return Forbid();
            }

            // Gate pass must exist before generating QR
            if (string.IsNullOrWhiteSpace(visitor.GatePassCode))
            {
                TempData["Error"] = "Gate pass code is not available for this visitor.";
                return RedirectToAction(nameof(Index));
            }

            // Generate QR from existing GatePassCode
            using var qrGenerator = new QRCodeGenerator();

            using var qrCodeData = qrGenerator.CreateQrCode(
                visitor.GatePassCode,
                QRCodeGenerator.ECCLevel.Q);

            var qrCode = new PngByteQRCode(qrCodeData);

            byte[] qrCodeImage = qrCode.GetGraphic(20);

            ViewBag.QrCode = Convert.ToBase64String(qrCodeImage);

            return View(visitor);
        }

        // =========================================================
        // GET: Visitor/Create
        // =========================================================

        public async Task<IActionResult> Create()
        {
            // Admin/Security can select any flat
            if (User.IsInRole("Admin") ||
                User.IsInRole("SecurityStaff"))
            {
                await LoadAllFlatsAsync();
                return View();
            }

            // Resident can create visitor only for own flat
            var resident = await GetCurrentResidentAsync();

            if (resident == null)
                return Forbid();

            var model = new Visitor
            {
                FlatId = resident.FlatId
            };

            await LoadOwnFlatAsync(resident.FlatId);

            return View(model);
        }


        // =========================================================
        // POST: Visitor/Create
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Visitor visitor)
        {
            // -----------------------------------------------------
            // Resident ownership protection
            // -----------------------------------------------------

            if (User.IsInRole("Resident"))
            {
                var resident = await GetCurrentResidentAsync();

                if (resident == null)
                    return Forbid();

                // Never trust FlatId or GatePassCode from the browser.
                visitor.FlatId = resident.FlatId;
                visitor.GatePassCode = await GenerateUniqueGatePassAsync();
                visitor.IsApproved = true;
            }


            // -----------------------------------------------------
            // Model validation
            // -----------------------------------------------------

            if (!ModelState.IsValid)
            {
                await LoadFlatsForCurrentUserAsync(visitor.FlatId);
                return View(visitor);
            }


            // -----------------------------------------------------
            // Validate flat exists
            // -----------------------------------------------------

            var flatExists = await _context.Flats
                .AnyAsync(f => f.Id == visitor.FlatId);

            if (!flatExists)
            {
                ModelState.AddModelError(
                    nameof(visitor.FlatId),
                    "Selected flat does not exist.");

                await LoadFlatsForCurrentUserAsync(visitor.FlatId);
                return View(visitor);
            }


            // -----------------------------------------------------
            // Validate Resident flat ownership
            // -----------------------------------------------------

            if (User.IsInRole("Resident"))
            {
                var resident = await GetCurrentResidentAsync();

                if (resident == null)
                    return Forbid();

                if (visitor.FlatId != resident.FlatId)
                    return Forbid();
            }


            // -----------------------------------------------------
            // Validate Gate Pass uniqueness
            // -----------------------------------------------------

            bool gatePassExists = await _context.Visitors
                .AnyAsync(v =>
                    v.GatePassCode == visitor.GatePassCode);

            if (gatePassExists)
            {
                ModelState.AddModelError(
                    nameof(visitor.GatePassCode),
                    "Gate pass code must be unique.");

                await LoadFlatsForCurrentUserAsync(visitor.FlatId);
                return View(visitor);
            }


            // -----------------------------------------------------
            // Validate date/time
            // -----------------------------------------------------

            if (visitor.ValidFrom >= visitor.ValidUntil)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Valid until must be after valid from.");

                await LoadFlatsForCurrentUserAsync(visitor.FlatId);
                return View(visitor);
            }


            // -----------------------------------------------------
            // Save visitor
            // -----------------------------------------------------

            _context.Visitors.Add(visitor);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Unable to save visitor. Please try again.");

                await LoadFlatsForCurrentUserAsync(visitor.FlatId);
                return View(visitor);
            }


            // -----------------------------------------------------
            // Audit
            // -----------------------------------------------------

            var userId = GetCurrentUserId();

            await _auditService.LogAsync(
                userId,
                "Create",
                "Visitor",
                visitor.Id.ToString(),
                $"Name:{visitor.VisitorName};GatePass:{visitor.GatePassCode}"
            );


            TempData["Success"] =
                "Visitor created successfully.";

            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // GET: Visitor/Edit/5
        // =========================================================

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var visitor = await _context.Visitors
                .FirstOrDefaultAsync(v => v.Id == id);

            if (visitor == null)
                return NotFound();


            // -----------------------------------------------------
            // Admin/Security can edit all
            // -----------------------------------------------------

            if (User.IsInRole("Admin") ||
                User.IsInRole("SecurityStaff"))
            {
                await LoadAllFlatsAsync();
                return View(visitor);
            }


            // -----------------------------------------------------
            // Resident can edit only own flat visitor
            // -----------------------------------------------------

            var resident = await GetCurrentResidentAsync();

            if (resident == null)
                return Forbid();

            if (visitor.FlatId != resident.FlatId)
                return Forbid();


            await LoadOwnFlatAsync(resident.FlatId);

            return View(visitor);
        }


        // =========================================================
        // POST: Visitor/Edit/5
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Visitor visitor)
        {
            if (id != visitor.Id)
                return NotFound();


            // -----------------------------------------------------
            // Get existing visitor from database
            // -----------------------------------------------------

            var existingVisitor = await _context.Visitors
                .FirstOrDefaultAsync(v => v.Id == id);

            if (existingVisitor == null)
                return NotFound();


            // -----------------------------------------------------
            // Resident ownership protection
            // -----------------------------------------------------

            if (User.IsInRole("Resident"))
            {
                var resident = await GetCurrentResidentAsync();

                if (resident == null)
                    return Forbid();

                // Resident can edit only own flat visitor
                if (existingVisitor.FlatId != resident.FlatId)
                    return Forbid();

                // Resident cannot move visitor to another flat or change approval state.
                visitor.FlatId = resident.FlatId;
                visitor.IsApproved = existingVisitor.IsApproved;
            }


            // -----------------------------------------------------
            // Model validation
            // -----------------------------------------------------

            if (!ModelState.IsValid)
            {
                await LoadFlatsForCurrentUserAsync(visitor.FlatId);
                return View(visitor);
            }


            // -----------------------------------------------------
            // Validate flat
            // -----------------------------------------------------

            var flatExists = await _context.Flats
                .AnyAsync(f => f.Id == visitor.FlatId);

            if (!flatExists)
            {
                ModelState.AddModelError(
                    nameof(visitor.FlatId),
                    "Selected flat does not exist.");

                await LoadFlatsForCurrentUserAsync(visitor.FlatId);
                return View(visitor);
            }


            // -----------------------------------------------------
            // Resident cannot change ownership
            // -----------------------------------------------------

            if (User.IsInRole("Resident"))
            {
                var resident = await GetCurrentResidentAsync();

                if (resident == null)
                    return Forbid();

                if (visitor.FlatId != resident.FlatId)
                    return Forbid();
            }


            // -----------------------------------------------------
            // Gate pass uniqueness
            // -----------------------------------------------------

            bool gatePassExists = await _context.Visitors
                .AnyAsync(v =>
                    v.Id != visitor.Id &&
                    v.GatePassCode == visitor.GatePassCode);

            if (gatePassExists)
            {
                ModelState.AddModelError(
                    nameof(visitor.GatePassCode),
                    "Gate pass code must be unique.");

                await LoadFlatsForCurrentUserAsync(visitor.FlatId);
                return View(visitor);
            }


            // -----------------------------------------------------
            // Date/time validation
            // -----------------------------------------------------

            if (visitor.ValidFrom >= visitor.ValidUntil)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Valid until must be after valid from.");

                await LoadFlatsForCurrentUserAsync(visitor.FlatId);
                return View(visitor);
            }


            // -----------------------------------------------------
            // Update
            // -----------------------------------------------------

            existingVisitor.FlatId = visitor.FlatId;
            existingVisitor.VisitorName = visitor.VisitorName;
            existingVisitor.Phone = visitor.Phone;
            existingVisitor.VehicleNumber = visitor.VehicleNumber;
            existingVisitor.GatePassCode = visitor.GatePassCode;
            existingVisitor.ValidFrom = visitor.ValidFrom;
            existingVisitor.ValidUntil = visitor.ValidUntil;
            existingVisitor.IsApproved = visitor.IsApproved;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VisitorExists(visitor.Id))
                    return NotFound();

                throw;
            }


            // -----------------------------------------------------
            // Audit
            // -----------------------------------------------------

            var userId = GetCurrentUserId();

            await _auditService.LogAsync(
                userId,
                "Update",
                "Visitor",
                visitor.Id.ToString(),
                $"Name:{visitor.VisitorName};GatePass:{visitor.GatePassCode}"
            );


            TempData["Success"] =
                "Visitor updated successfully.";

            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // GET: Visitor/Delete/5
        // =========================================================

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var visitor = await _context.Visitors
                .Include(v => v.Flat)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (visitor == null)
                return NotFound();


            // Admin/Security can delete all
            if (User.IsInRole("Admin") ||
                User.IsInRole("SecurityStaff"))
            {
                return View(visitor);
            }


            // Resident can delete only own flat visitor
            var resident = await GetCurrentResidentAsync();

            if (resident == null)
                return Forbid();

            if (visitor.FlatId != resident.FlatId)
                return Forbid();

            return View(visitor);
        }


        // =========================================================
        // POST: Visitor/Delete/5
        // =========================================================

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var visitor = await _context.Visitors
                .FirstOrDefaultAsync(v => v.Id == id);

            if (visitor == null)
                return NotFound();


            // -----------------------------------------------------
            // Resident ownership protection
            // -----------------------------------------------------

            if (User.IsInRole("Resident"))
            {
                var resident = await GetCurrentResidentAsync();

                if (resident == null)
                    return Forbid();

                if (visitor.FlatId != resident.FlatId)
                    return Forbid();
            }


            // -----------------------------------------------------
            // Store audit information before delete
            // -----------------------------------------------------

            var visitorName = visitor.VisitorName;
            var gatePass = visitor.GatePassCode;


            // -----------------------------------------------------
            // Delete
            // -----------------------------------------------------

            _context.Visitors.Remove(visitor);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                TempData["Error"] =
                    "Visitor cannot be deleted because related records exist.";

                return RedirectToAction(nameof(Index));
            }


            // -----------------------------------------------------
            // Audit
            // -----------------------------------------------------

            var userId = GetCurrentUserId();

            await _auditService.LogAsync(
                userId,
                "Delete",
                "Visitor",
                id.ToString(),
                $"Name:{visitorName};GatePass:{gatePass}"
            );


            TempData["Success"] =
                "Visitor deleted successfully.";

            return RedirectToAction(nameof(Index));
        }



        private async Task<string> GenerateUniqueGatePassAsync()
        {
            var random = Random.Shared;
            for (var attempt = 0; attempt < 50; attempt++)
            {
                var code = random.Next(100000, 1000000).ToString();
                if (!await _context.Visitors.AnyAsync(v => v.GatePassCode == code))
                    return code;
            }
            throw new InvalidOperationException("Unable to generate a unique gate pass code.");
        }

        // =========================================================
        // Helper: Get Current Resident
        // =========================================================

        private async Task<ResidentProfile?> GetCurrentResidentAsync()
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return null;

            return await _context.ResidentProfiles
                .FirstOrDefaultAsync(r =>
                    r.ApplicationUserId == userId);
        }


        // =========================================================
        // Helper: Current User ID
        // =========================================================

        private string GetCurrentUserId()
        {
            return User.FindFirstValue(
                ClaimTypes.NameIdentifier)
                ?? string.Empty;
        }


        // =========================================================
        // Helper: Check Visitor Exists
        // =========================================================

        private bool VisitorExists(int id)
        {
            return _context.Visitors
                .Any(v => v.Id == id);
        }


        // =========================================================
        // Helper: Load All Flats
        // =========================================================

        private async Task LoadAllFlatsAsync()
        {
            var flats = await _context.Flats
                .OrderBy(f => f.BlockName)
                .ThenBy(f => f.FlatNumber)
                .ToListAsync();

            ViewBag.Flats = flats
                .Select(f => new SelectListItem
                {
                    Value = f.Id.ToString(),
                    Text = $"{f.BlockName} - {f.FlatNumber}"
                })
                .ToList();
        }


        // =========================================================
        // Helper: Load Own Flat
        // =========================================================

        private async Task LoadOwnFlatAsync(int flatId)
        {
            var flat = await _context.Flats
                .FirstOrDefaultAsync(f => f.Id == flatId);

            if (flat == null)
            {
                ViewBag.Flats =
                    new List<SelectListItem>();

                return;
            }

            ViewBag.Flats =
                new List<SelectListItem>
                {
                    new SelectListItem
                    {
                        Value = flat.Id.ToString(),
                        Text = $"{flat.BlockName} - {flat.FlatNumber}",
                        Selected = true
                    }
                };
        }


        // =========================================================
        // Helper: Load Flats According To Current Role
        // =========================================================

        private async Task LoadFlatsForCurrentUserAsync(
            int? selectedFlatId = null)
        {
            if (User.IsInRole("Admin") ||
                User.IsInRole("SecurityStaff"))
            {
                await LoadAllFlatsAsync();
                return;
            }

            var resident = await GetCurrentResidentAsync();

            if (resident == null)
            {
                ViewBag.Flats =
                    new List<SelectListItem>();

                return;
            }

            await LoadOwnFlatAsync(resident.FlatId);
        }
    }
}