using Microsoft.AspNetCore.Identity;
using SmartSociety.Models;

namespace SmartSociety.Data;

public class ApplicationUser : IdentityUser
{
    public ResidentProfile? ResidentProfile { get; set; }
}