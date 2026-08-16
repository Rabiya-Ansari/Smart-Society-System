using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartSociety.Data;
using SmartSociety.Models;

namespace SmartSociety.Controllers
{
    [Authorize(Roles = "Admin")]
    public class BillItemController : Controller
    {
        private readonly AppDbContext _context;

        public BillItemController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Create(int maintenanceBillId)
        {
            var bill = await _context.MaintenanceBills.FindAsync(maintenanceBillId);
            if (bill == null) return NotFound();

            ViewBag.MaintenanceBillId = maintenanceBillId;
            return View(new BillItem { MaintenanceBillId = maintenanceBillId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BillItem model)
        {
            if (!ModelState.IsValid) return View(model);

            var bill = await _context.MaintenanceBills.FindAsync(model.MaintenanceBillId);
            if (bill == null)
            {
                ModelState.AddModelError(nameof(model.MaintenanceBillId), "Maintenance bill not found.");
                return View(model);
            }

            _context.BillItems.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Bill item added.";
            return RedirectToAction("Details", "Maintenance", new { id = model.MaintenanceBillId });
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.BillItems.Include(b => b.MaintenanceBill).FirstOrDefaultAsync(b => b.Id == id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.BillItems.FindAsync(id);
            if (item == null) return NotFound();

            var billId = item.MaintenanceBillId;
            _context.BillItems.Remove(item);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Bill item removed.";
            return RedirectToAction("Details", "Maintenance", new { id = billId });
        }
    }
}
