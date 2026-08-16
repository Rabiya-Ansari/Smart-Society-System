using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartSociety.Data;

var builder = WebApplication.CreateBuilder(args);


// =====================================================
// Database Connection
// =====================================================

var connectionString =
    builder.Configuration.GetConnectionString("Connect")
    ?? throw new InvalidOperationException(
        "Connection string 'Connect' not found."
    );

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));


// =====================================================
// ASP.NET Core Identity
// =====================================================

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultUI()
.AddDefaultTokenProviders();


// =====================================================
// MVC + Razor Pages
// =====================================================

builder.Services.AddControllersWithViews();

builder.Services.AddRazorPages();

// HTTP context accessor and audit logging
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<SmartSociety.Services.IAuditService, SmartSociety.Services.AuditService>();


var app = builder.Build();


// =====================================================
// Seed Roles & Admin User
// =====================================================

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Apply database schema before Identity seeding.
    await db.Database.MigrateAsync();
    await DatabaseSchemaInitializer.EnsureComplaintColumnsAsync(db);

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    await RoleSeeder.SeedRolesAsync(roleManager);
    await AdminSeeder.SeedAdminAsync(userManager);

    // Security and Maintenance accounts are system accounts.
    // Residents are intentionally created by Admin, not hardcoded.
    await DemoDataSeeder.SeedStaffAsync(userManager);
}


// =====================================================
// HTTP Request Pipeline
// =====================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();

app.UseRouting();


// =====================================================
// Authentication & Authorization
// =====================================================

app.UseAuthentication();

app.UseAuthorization();


// =====================================================
// Razor Pages / Identity UI
// =====================================================

app.MapRazorPages();


// =====================================================
// Static Assets
// =====================================================

app.MapStaticAssets();


// =====================================================
// MVC Default Route
// =====================================================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();