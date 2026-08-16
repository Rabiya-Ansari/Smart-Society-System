using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartSociety.Models;

namespace SmartSociety.Controllers
{
    [Authorize(Roles = "Admin,Resident")]
    public class PollController : Controller
    {
        private readonly AppDbContext _context;
        public PollController(AppDbContext context)=>_context=context;
        public async Task<IActionResult> Index(){var q=_context.Polls.Include(p=>p.PollOptions).Include(p=>p.PollVotes).AsQueryable();if(User.IsInRole("Resident"))q=q.Where(p=>p.IsActive&&p.StartDate<=DateTime.Now&&p.EndDate>=DateTime.Now);return View(await q.OrderByDescending(p=>p.StartDate).ToListAsync());}
        [Authorize(Roles="Admin")] public IActionResult Create()=>View();
        [HttpPost,ValidateAntiForgeryToken,Authorize(Roles="Admin")]
        public async Task<IActionResult> Create(Poll model){if(!ModelState.IsValid)return View(model);if(model.StartDate>=model.EndDate){ModelState.AddModelError(nameof(model.EndDate),"End date must be after start date.");return View(model);}_context.Polls.Add(model);await _context.SaveChangesAsync();TempData["Success"]="Poll created.";return RedirectToAction(nameof(Index));}
        public async Task<IActionResult> Details(int? id){if(id==null)return NotFound();var p=await _context.Polls.Include(x=>x.PollOptions).Include(x=>x.PollVotes).FirstOrDefaultAsync(x=>x.Id==id);if(p==null)return NotFound();if(User.IsInRole("Resident")&&(!p.IsActive||p.StartDate>DateTime.Now||p.EndDate<DateTime.Now))return NotFound();return View(p);}
        [Authorize(Roles="Admin")] public async Task<IActionResult> Delete(int? id){if(id==null)return NotFound();var p=await _context.Polls.FindAsync(id);return p==null?NotFound():View(p);}
        [HttpPost,ActionName("Delete"),ValidateAntiForgeryToken,Authorize(Roles="Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id){var p=await _context.Polls.FindAsync(id);if(p==null)return NotFound();if(await _context.PollVotes.AnyAsync(v=>v.PollId==id)){TempData["Error"]="Cannot delete a poll with votes.";return RedirectToAction(nameof(Index));}_context.Polls.Remove(p);await _context.SaveChangesAsync();TempData["Success"]="Poll deleted.";return RedirectToAction(nameof(Index));}
    }
}
