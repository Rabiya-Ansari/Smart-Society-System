using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartSociety.Data;
using SmartSociety.Models;

namespace SmartSociety.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AuditLogController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public AuditLogController(AppDbContext context, UserManager<ApplicationUser> userManager){_context=context;_userManager=userManager;}

        public async Task<IActionResult> Index(string? search, string? actionType, string? entityName, string? userId, DateTime? startDate, DateTime? endDate, int page=1)
        {
            const int pageSize=30; if(page<1)page=1;
            var q=_context.AuditLogs.Include(a=>a.ApplicationUser).AsNoTracking().AsQueryable();
            if(!string.IsNullOrWhiteSpace(search))q=q.Where(a=>(a.Details??"").Contains(search)||(a.EntityId??"").Contains(search));
            if(!string.IsNullOrWhiteSpace(actionType))q=q.Where(a=>a.Action==actionType);
            if(!string.IsNullOrWhiteSpace(entityName))q=q.Where(a=>a.EntityName==entityName);
            if(!string.IsNullOrWhiteSpace(userId))q=q.Where(a=>a.ApplicationUserId==userId);
            if(startDate.HasValue)q=q.Where(a=>a.Timestamp>=startDate.Value.Date);
            if(endDate.HasValue)q=q.Where(a=>a.Timestamp<endDate.Value.Date.AddDays(1));
            var total=await q.CountAsync();
            var logs=await q.OrderByDescending(a=>a.Timestamp).Skip((page-1)*pageSize).Take(pageSize).ToListAsync();
            ViewBag.Users=await _userManager.Users.OrderBy(u=>u.Email).Select(u=>new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem{Value=u.Id,Text=u.Email??u.UserName??u.Id}).ToListAsync();
            ViewBag.Actions=await _context.AuditLogs.Select(a=>a.Action).Distinct().OrderBy(x=>x).ToListAsync();
            ViewBag.Entities=await _context.AuditLogs.Where(a=>a.EntityName!=null).Select(a=>a.EntityName!).Distinct().OrderBy(x=>x).ToListAsync();
            ViewBag.Page=page;ViewBag.PageSize=pageSize;ViewBag.Total=total;ViewBag.Search=search;ViewBag.ActionType=actionType;ViewBag.EntityName=entityName;ViewBag.UserId=userId;ViewBag.StartDate=startDate;ViewBag.EndDate=endDate;
            return View(logs);
        }
    }
}
