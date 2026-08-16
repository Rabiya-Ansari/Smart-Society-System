using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartSociety.Data;
using SmartSociety.Models;

namespace SmartSociety.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PollOptionController : Controller
    {
        private readonly AppDbContext _context;

        public PollOptionController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Create(int pollId)
        {
            ViewBag.PollId = pollId;
            return View(new PollOption { PollId = pollId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PollOption model)
        {
            if (!ModelState.IsValid) return View(model);

            var poll = await _context.Polls.FindAsync(model.PollId);
            if (poll == null)
            {
                ModelState.AddModelError(nameof(model.PollId), "Selected poll does not exist.");
                return View(model);
            }

            _context.PollOptions.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Option added.";
            return RedirectToAction("Details", "Poll", new { id = model.PollId });
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var option = await _context.PollOptions.Include(p => p.Poll).FirstOrDefaultAsync(p => p.Id == id);
            if (option == null) return NotFound();
            return View(option);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var option = await _context.PollOptions.FindAsync(id);
            if (option == null) return NotFound();

            // Prevent delete if votes exist for this option
            var hasVotes = await _context.PollVotes.AnyAsync(v => v.PollOptionId == id);
            if (hasVotes)
            {
                TempData["Error"] = "Cannot delete option that has votes.";
                return RedirectToAction("Details", "Poll", new { id = option.PollId });
            }

            _context.PollOptions.Remove(option);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Option deleted.";
            return RedirectToAction("Details", "Poll", new { id = option.PollId });
        }
    }
}
