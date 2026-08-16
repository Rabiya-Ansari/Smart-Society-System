using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartSociety.Models;

namespace SmartSociety.Controllers
{
    [Authorize(Roles = "Admin,Resident")]
    public class NoticeController : Controller
    {
        private readonly AppDbContext _context;
        public NoticeController(AppDbContext context)=>_context=context;
        public async Task<IActionResult> Index(){var q=_context.Notices.AsQueryable();if(User.IsInRole("Resident"))q=q.Where(n=>n.IsPublished&&n.PublishDate<=DateTime.Now&&n.ExpiryDate>DateTime.Now);return View(await q.OrderByDescending(n=>n.PublishDate).ToListAsync());}
        [Authorize(Roles="Admin")] public IActionResult Create()=>View();
        [HttpPost,ValidateAntiForgeryToken,Authorize(Roles="Admin")]
        public async Task<IActionResult> Create(Notice model){if(!ModelState.IsValid)return View(model);if(model.PublishDate>=model.ExpiryDate){ModelState.AddModelError(nameof(model.ExpiryDate),"Expiry date must be after publish date.");return View(model);}_context.Notices.Add(model);await _context.SaveChangesAsync();TempData["Success"]="Notice created.";return RedirectToAction(nameof(Index));}
        [Authorize(Roles="Admin")] public async Task<IActionResult> Edit(int? id){if(id==null)return NotFound();var n=await _context.Notices.FindAsync(id);return n==null?NotFound():View(n);}
        [HttpPost,ValidateAntiForgeryToken,Authorize(Roles="Admin")]
        public async Task<IActionResult> Edit(int id,Notice model){if(id!=model.Id)return NotFound();if(!ModelState.IsValid)return View(model);if(model.PublishDate>=model.ExpiryDate){ModelState.AddModelError(nameof(model.ExpiryDate),"Expiry date must be after publish date.");return View(model);}_context.Update(model);await _context.SaveChangesAsync();TempData["Success"]="Notice updated.";return RedirectToAction(nameof(Index));}
        [Authorize(Roles="Admin")] public async Task<IActionResult> Delete(int? id){if(id==null)return NotFound();var n=await _context.Notices.FindAsync(id);return n==null?NotFound():View(n);}
        [HttpPost,ActionName("Delete"),ValidateAntiForgeryToken,Authorize(Roles="Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id){var n=await _context.Notices.FindAsync(id);if(n==null)return NotFound();_context.Notices.Remove(n);await _context.SaveChangesAsync();TempData["Success"]="Notice deleted.";return RedirectToAction(nameof(Index));}
    }
}
