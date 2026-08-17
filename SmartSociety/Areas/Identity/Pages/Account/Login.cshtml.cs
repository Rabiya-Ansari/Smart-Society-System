// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using SmartSociety.Data;

namespace SmartSociety.Areas.Identity.Pages.Account;

public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<LoginModel> _logger;

    public LoginModel(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ILogger<LoginModel> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = default!;

    public IList<AuthenticationScheme>? ExternalLogins { get; set; }

    public string? ReturnUrl { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = default!;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = default!;

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(string? returnUrl = null)
    {
        // Already logged-in user should never see the Login page
        if (User.Identity?.IsAuthenticated == true)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);

                // Admin
                if (roles.Any(r => r.Equals("Admin", StringComparison.OrdinalIgnoreCase)))
                {
                    return RedirectToAction("Index", "Admin");
                }

                // Resident / Homeowner
                if (roles.Any(r =>
                    r.Equals("Resident", StringComparison.OrdinalIgnoreCase) ||
                    r.Equals("Homeowner", StringComparison.OrdinalIgnoreCase)))
                {
                    return RedirectToAction("Index", "Home");
                }

                // Security / Guard
                if (roles.Any(r =>
                    r.Equals("Security", StringComparison.OrdinalIgnoreCase) ||
                    r.Equals("Guard", StringComparison.OrdinalIgnoreCase)))
                {
                    return RedirectToAction("Index", "Security");
                }
            }

            // Fallback for authenticated user with no recognized role
            return RedirectToAction("Index", "Home");
        }

        if (!string.IsNullOrEmpty(ErrorMessage))
        {
            ModelState.AddModelError(string.Empty, ErrorMessage);
        }

        returnUrl ??= Url.Content("~/");

        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        ExternalLogins =
            (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

        ReturnUrl = returnUrl;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in.");

                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user != null)
                {
                    var roles = await _userManager.GetRolesAsync(user);

                    // 1. Admin Role Redirect
                    if (roles.Any(r => r.Equals("Admin", StringComparison.OrdinalIgnoreCase)))
                    {
                        return RedirectToAction("Index", "Admin");
                    }

                    // 2. Resident Role Redirect (ResidentController par navigate karega)
                    if (roles.Any(r => r.Equals("Resident", StringComparison.OrdinalIgnoreCase) ||
                                       r.Equals("Homeowner", StringComparison.OrdinalIgnoreCase)))
                    {
                        return RedirectToAction("Index", "Home");
                    }

                    // 3. Security / Guard Role Redirect (SecurityController par navigate karega)
                    if (roles.Any(r => r.Equals("Security", StringComparison.OrdinalIgnoreCase) ||
                                       r.Equals("Guard", StringComparison.OrdinalIgnoreCase)))
                    {
                        return RedirectToAction("Index", "Security");
                    }
                }

                // Normal ReturnUrl handling if valid and safe
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) &&
                    !returnUrl.Contains("Logout", StringComparison.OrdinalIgnoreCase) && returnUrl != "/")
                {
                    return LocalRedirect(returnUrl);
                }

                // Fallback route
                return RedirectToAction("Index", "Home");
            }

            if (result.RequiresTwoFactor)
            {
                return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out.");
                return RedirectToPage("./Lockout");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }
        }

        return Page();
    }
}