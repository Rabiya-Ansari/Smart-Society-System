using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartSociety.Data;
using SmartSociety.Models;
using SmartSociety.Models.Enums;

namespace SmartSociety.Controllers
{
    [Authorize(Roles = "Admin,Resident")]
    public class MaintenanceController : Controller
    {
        private readonly AppDbContext _context; private readonly UserManager<ApplicationUser> _userManager;
        public MaintenanceController(AppDbContext context, UserManager<ApplicationUser> userManager){_context=context;_userManager=userManager;}
        public async Task<IActionResult> Index(){var q=_context.MaintenanceBills.Include(b=>b.Flat).AsQueryable();if(User.IsInRole("Resident")){var r=await GetResidentAsync();if(r==null)return Forbid();q=q.Where(b=>b.FlatId==r.FlatId);}return View(await q.OrderByDescending(b=>b.BillingMonth).ToListAsync());}
        public async Task<IActionResult> Details(int? id){if(id==null)return NotFound();var bill=await _context.MaintenanceBills.Include(b=>b.Flat).Include(b=>b.BillItems).Include(b=>b.Payments).FirstOrDefaultAsync(b=>b.Id==id);if(bill==null)return NotFound();if(User.IsInRole("Resident")){var r=await GetResidentAsync();if(r==null||bill.FlatId!=r.FlatId)return Forbid();}return View(bill);}
        [Authorize(Roles="Admin")] public async Task<IActionResult> Create(){await LoadFlats();return View();}
        [HttpPost,ValidateAntiForgeryToken,Authorize(Roles="Admin")]
        public async Task<IActionResult> Create(MaintenanceBill model){if(!ModelState.IsValid){await LoadFlats();return View(model);}if(!await _context.Flats.AnyAsync(f=>f.Id==model.FlatId)){ModelState.AddModelError(nameof(model.FlatId),"Selected flat does not exist.");await LoadFlats();return View(model);} _context.MaintenanceBills.Add(model);await _context.SaveChangesAsync();TempData["Success"]="Maintenance bill created.";return RedirectToAction(nameof(Index));}
        [Authorize(Roles="Admin")] public async Task<IActionResult> Delete(int? id){if(id==null)return NotFound();var b=await _context.MaintenanceBills.Include(x=>x.Flat).FirstOrDefaultAsync(x=>x.Id==id);return b==null?NotFound():View(b);}
        [HttpPost,ActionName("Delete"),ValidateAntiForgeryToken,Authorize(Roles="Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id){var b=await _context.MaintenanceBills.Include(x=>x.Payments).FirstOrDefaultAsync(x=>x.Id==id);if(b==null)return NotFound();if(b.Payments.Any()){TempData["Error"]="Cannot delete a bill that has payments.";return RedirectToAction(nameof(Index));}_context.MaintenanceBills.Remove(b);await _context.SaveChangesAsync();TempData["Success"]="Maintenance bill deleted.";return RedirectToAction(nameof(Index));}
        private async Task<ResidentProfile?> GetResidentAsync(){var u=await _userManager.GetUserAsync(User);return u==null?null:await _context.ResidentProfiles.FirstOrDefaultAsync(r=>r.ApplicationUserId==u.Id);}
        private async Task LoadFlats(){ViewBag.Flats=new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _context.Flats.OrderBy(f=>f.BlockName).ThenBy(f=>f.FlatNumber).ToListAsync(),"Id","FlatNumber");}
    }
}
