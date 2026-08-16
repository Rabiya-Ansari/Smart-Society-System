using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartSociety.Data;

var builder = WebApplication.CreateBuilder(args);

// =====================================================
// DATABASE
// =====================================================

var connectionString =
    builder.Configuration.GetConnectionString("Connect")
    ?? throw new InvalidOperationException(
        "Connection string 'Connect' not found."
    );

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));


// =====================================================
// ASP.NET CORE IDENTITY
// =====================================================

builder.Services
    .AddDefaultIdentity<ApplicationUser>(options =>
    {
        // Development / SRS project
        // Email confirmation required nahi hogi
        options.SignIn.RequireConfirmedAccount = false;

        // Login security
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan =
            TimeSpan.FromMinutes(15);
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultUI()
    .AddDefaultTokenProviders();


// =====================================================
// MVC
// =====================================================

builder.Services.AddControllersWithViews();

builder.Services.AddRazorPages();


// =====================================================
// SERVICES
// =====================================================

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<
    SmartSociety.Services.IAuditService,
    SmartSociety.Services.AuditService
>();


var app = builder.Build();


// =====================================================
// DATABASE + SEEDING
// =====================================================

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var db =
        services.GetRequiredService<AppDbContext>();

    // Apply migrations
    await db.Database.MigrateAsync();

    // Complaint columns
    await DatabaseSchemaInitializer
        .EnsureComplaintColumnsAsync(db);


    // =================================================
    // ROLES
    // =================================================

    var roleManager =
        services.GetRequiredService<
            RoleManager<IdentityRole>>();

    await RoleSeeder.SeedRolesAsync(
        roleManager
    );


    // =================================================
    // ADMIN
    // =================================================

    var userManager =
        services.GetRequiredService<
            UserManager<ApplicationUser>>();

    await AdminSeeder.SeedAdminAsync(
        userManager
    );


    // =================================================
    // STAFF / SECURITY DEMO USERS
    // =================================================

    await DemoDataSeeder.SeedStaffAsync(
        userManager
    );
}


// =====================================================
// HTTP PIPELINE
// =====================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");

    app.UseHsts();
}


app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();


// =====================================================
// AUTHENTICATION
// =====================================================

app.UseAuthentication();


// =====================================================
// AUTHORIZATION
// =====================================================

app.UseAuthorization();


// =====================================================
// IDENTITY UI
// =====================================================

app.MapRazorPages();


// =====================================================
// STATIC ASSETS
// =====================================================

app.MapStaticAssets();


// =====================================================
// DEFAULT MVC ROUTE
// =====================================================

app.MapControllerRoute(
    name: "default",
    pattern:
        "{controller=Home}/{action=Index}/{id?}"
)
.WithStaticAssets();


app.Run();