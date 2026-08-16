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
    [Authorize(Roles = "Admin,SecurityStaff")]
    public class GateLogController : Controller
    {
        private readonly AppDbContext _context; private readonly UserManager<ApplicationUser> _userManager;
        public GateLogController(AppDbContext context, UserManager<ApplicationUser> userManager){_context=context;_userManager=userManager;}
        public async Task<IActionResult> Index(){var q=_context.GateLogs.Include(g=>g.Visitor).Include(g=>g.SecurityGuard).AsQueryable();return View(await q.OrderByDescending(g=>g.EntryTime).ToListAsync());}
        public async Task<IActionResult> Details(int? id){if(id==null)return NotFound();var x=await _context.GateLogs.Include(g=>g.Visitor).ThenInclude(v=>v.Flat).Include(g=>g.SecurityGuard).FirstOrDefaultAsync(g=>g.Id==id);return x==null?NotFound():View(x);}
        [Authorize(Roles="Admin")] public async Task<IActionResult> Create(){await LoadLookups();return View();}
        [HttpPost,ValidateAntiForgeryToken,Authorize(Roles="Admin")]
        public async Task<IActionResult> Create(GateLog log){if(!ModelState.IsValid){await LoadLookups();return View(log);}if(!await _context.Visitors.AnyAsync(v=>v.Id==log.VisitorId)){ModelState.AddModelError(nameof(log.VisitorId),"Visitor does not exist.");await LoadLookups();return View(log);}if(!await _userManager.Users.AnyAsync(u=>u.Id==log.SecurityGuardId)){ModelState.AddModelError(nameof(log.SecurityGuardId),"Security guard does not exist.");await LoadLookups();return View(log);}_context.GateLogs.Add(log);await _context.SaveChangesAsync();return RedirectToAction(nameof(Index));}
        [Authorize(Roles="Admin")] public async Task<IActionResult> Edit(int? id){if(id==null)return NotFound();var x=await _context.GateLogs.FindAsync(id);if(x==null)return NotFound();await LoadLookups();return View(x);}
        [HttpPost,ValidateAntiForgeryToken,Authorize(Roles="Admin")]
        public async Task<IActionResult> Edit(int id,GateLog log){if(id!=log.Id)return NotFound();if(!ModelState.IsValid){await LoadLookups();return View(log);}_context.Update(log);await _context.SaveChangesAsync();return RedirectToAction(nameof(Index));}
        [Authorize(Roles="Admin")] public async Task<IActionResult> Delete(int? id){if(id==null)return NotFound();var x=await _context.GateLogs.Include(g=>g.Visitor).FirstOrDefaultAsync(g=>g.Id==id);return x==null?NotFound():View(x);}
        [HttpPost,ActionName("Delete"),ValidateAntiForgeryToken,Authorize(Roles="Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id){var x=await _context.GateLogs.FindAsync(id);if(x==null)return NotFound();_context.GateLogs.Remove(x);await _context.SaveChangesAsync();return RedirectToAction(nameof(Index));}
        private async Task LoadLookups(){ViewBag.Visitors=new SelectList(await _context.Visitors.Include(v=>v.Flat).OrderByDescending(v=>v.ValidFrom).ToListAsync(),"Id","VisitorName");ViewBag.SecurityGuards=(await _userManager.GetUsersInRoleAsync("SecurityStaff")).Select(u=>new SelectListItem{Value=u.Id,Text=u.Email??u.UserName??u.Id}).ToList();ViewBag.Statuses=new SelectList(Enum.GetValues<GateLogStatus>());}
    }
}
