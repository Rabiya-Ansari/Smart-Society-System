using Microsoft.AspNetCore.Identity;
using SmartSociety.Models;

namespace SmartSociety.Data;

public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }

    public bool IsActive { get; set; } = true;

    public ResidentProfile? ResidentProfile { get; set; }
}