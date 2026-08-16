using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartSociety.Data;
using SmartSociety.Models;
using SmartSociety.Models.Enums;
using SmartSociety.Services;

namespace SmartSociety.Controllers
{
    [Authorize(Roles = "Admin,Resident")]
    public class PaymentController : Controller
    {
        private readonly AppDbContext _context; private readonly UserManager<ApplicationUser> _userManager; private readonly IAuditService _audit;
        public PaymentController(AppDbContext context,UserManager<ApplicationUser> userManager,IAuditService audit){_context=context;_userManager=userManager;_audit=audit;}
        public async Task<IActionResult> Create(int billId){var bill=await _context.MaintenanceBills.Include(b=>b.Flat).Include(b=>b.Payments).FirstOrDefaultAsync(b=>b.Id==billId);if(bill==null)return NotFound();var user=await _userManager.GetUserAsync(User);if(user==null)return Challenge();if(!await OwnsBillAsync(user,bill))return Forbid();var model=new Payment{MaintenanceBillId=billId,ApplicationUserId=user.Id,PaymentMethod=PaymentMethod.SimulatedGateway,PaymentStatus=PaymentStatus.Pending};await LoadMethods();ViewBag.Bill=bill;return View(model);}
        [HttpPost,ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Payment model){var bill=await _context.MaintenanceBills.Include(b=>b.Flat).Include(b=>b.Payments).FirstOrDefaultAsync(b=>b.Id==model.MaintenanceBillId);if(bill==null)return NotFound();var user=await _userManager.GetUserAsync(User);if(user==null)return Challenge();if(!await OwnsBillAsync(user,bill))return Forbid();var outstanding=bill.AmountDue+bill.PenaltyAmount-bill.Payments.Where(p=>p.PaymentStatus!=PaymentStatus.Rejected).Sum(p=>p.Amount);if(outstanding<=0){TempData["Error"]="This bill has no outstanding balance.";return RedirectToAction("Details","Maintenance",new{id=bill.Id});}if(model.Amount<=0)ModelState.AddModelError(nameof(model.Amount),"Amount must be greater than 0.");if(model.Amount>outstanding)ModelState.AddModelError(nameof(model.Amount),$"Amount cannot exceed outstanding balance ({outstanding:N2}).");if(!ModelState.IsValid){await LoadMethods();ViewBag.Bill=bill;return View(model);}model.ApplicationUserId=user.Id;model.PaymentDate=DateTime.UtcNow;model.PaymentStatus=model.Amount>=outstanding?PaymentStatus.Paid:PaymentStatus.PartiallyPaid;_context.Payments.Add(model);await _context.SaveChangesAsync();var totalPaid=await _context.Payments.Where(p=>p.MaintenanceBillId==bill.Id&&p.PaymentStatus!=PaymentStatus.Rejected).SumAsync(p=>(double?)p.Amount)??0;bill.PaymentStatus=totalPaid>=bill.AmountDue+bill.PenaltyAmount?PaymentStatus.Paid:PaymentStatus.PartiallyPaid;await _context.SaveChangesAsync();await _audit.LogAsync(user.Id,"Create","Payment",model.Id.ToString(),$"Bill:{bill.Id};Amount:{model.Amount}");TempData["Success"]="Payment recorded successfully.";return RedirectToAction("Details","Maintenance",new{id=bill.Id});}
        private async Task<bool> OwnsBillAsync(ApplicationUser user,MaintenanceBill bill){if(User.IsInRole("Admin"))return true;var r=await _context.ResidentProfiles.FirstOrDefaultAsync(x=>x.ApplicationUserId==user.Id);return r!=null&&r.FlatId==bill.FlatId;}
        private async Task LoadMethods(){ViewBag.Methods=new SelectList(Enum.GetValues<PaymentMethod>().Select(m=>new SelectListItem{Value=((int)m).ToString(),Text=m.ToString()}),"Value","Text");await Task.CompletedTask;}
    }
}
