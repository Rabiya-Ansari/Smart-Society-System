using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartSociety.Models;

namespace SmartSociety.Controllers
{
    [Authorize(Roles = "Admin,Resident")]
    public class AmenityController : Controller
    {
        private readonly AppDbContext _context;
        public AmenityController(AppDbContext context) => _context = context;
        public async Task<IActionResult> Index() => View(await _context.Amenities.OrderBy(a => a.Name).ToListAsync());
        [Authorize(Roles="Admin")]
        public IActionResult Create() => View();
        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles="Admin")]
        public async Task<IActionResult> Create(Amenity model) { if (!ModelState.IsValid) return View(model); _context.Amenities.Add(model); await _context.SaveChangesAsync(); TempData["Success"]="Amenity created."; return RedirectToAction(nameof(Index)); }
        [Authorize(Roles="Admin")]
        public async Task<IActionResult> Edit(int? id) { if(id==null)return NotFound(); var a=await _context.Amenities.FindAsync(id); return a==null?NotFound():View(a); }
        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles="Admin")]
        public async Task<IActionResult> Edit(int id,Amenity model){if(id!=model.Id)return NotFound();if(!ModelState.IsValid)return View(model);_context.Update(model);await _context.SaveChangesAsync();TempData["Success"]="Amenity updated.";return RedirectToAction(nameof(Index));}
        [Authorize(Roles="Admin")]
        public async Task<IActionResult> Delete(int? id){if(id==null)return NotFound();var a=await _context.Amenities.FindAsync(id);return a==null?NotFound():View(a);}
        [HttpPost,ActionName("Delete"),ValidateAntiForgeryToken,Authorize(Roles="Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id){var a=await _context.Amenities.FindAsync(id);if(a==null)return NotFound();if(await _context.AmenityBookings.AnyAsync(b=>b.AmenityId==id)){TempData["Error"]="Cannot delete an amenity with existing bookings.";return RedirectToAction(nameof(Index));}_context.Amenities.Remove(a);await _context.SaveChangesAsync();TempData["Success"]="Amenity deleted.";return RedirectToAction(nameof(Index));}
    }
}
