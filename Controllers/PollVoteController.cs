using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartSociety.Data;
using SmartSociety.Models;

namespace SmartSociety.Controllers
{
    [Authorize(Roles = "Resident")]
    public class PollVoteController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PollVoteController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Vote(int pollId, int optionId)
        {
            var poll = await _context.Polls.Include(p => p.PollOptions).FirstOrDefaultAsync(p => p.Id == pollId);
            if (poll == null) return NotFound();

            if (!poll.IsActive || DateTime.UtcNow < poll.StartDate || DateTime.UtcNow > poll.EndDate)
            {
                TempData["Error"] = "Poll is not active.";
                return RedirectToAction("Details", "Poll", new { id = pollId });
            }

            var option = poll.PollOptions.FirstOrDefault(o => o.Id == optionId);
            if (option == null)
            {
                TempData["Error"] = "Invalid option.";
                return RedirectToAction("Details", "Poll", new { id = pollId });
            }

            var user = await _userManager.GetUserAsync(User);
            var resident = await _context.ResidentProfiles.FirstOrDefaultAsync(r => r.ApplicationUserId == user.Id);
            if (resident == null)
            {
                TempData["Error"] = "Resident profile not found.";
                return RedirectToAction("Index", "Home");
            }

            bool already = await _context.PollVotes.AnyAsync(v => v.PollId == pollId && v.ResidentProfileId == resident.Id);
            if (already)
            {
                TempData["Error"] = "You have already voted in this poll.";
                return RedirectToAction("Details", "Poll", new { id = pollId });
            }

            var vote = new PollVote
            {
                PollId = pollId,
                PollOptionId = optionId,
                ResidentProfileId = resident.Id,
                VotedAt = DateTime.UtcNow
            };

            _context.PollVotes.Add(vote);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Vote recorded.";
            return RedirectToAction("Details", "Poll", new { id = pollId });
        }
    }
}
