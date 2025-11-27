using GlowBook.Model.Data;
using GlowBook.Model.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=glowbook.db"));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Localisation
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

// Meertaligheid (NL/EN)
var supportedCultures = new[]
{
    new CultureInfo("nl"),
    new CultureInfo("en")
};

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("nl");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.RequestCultureProviders.Insert(0,
        new CookieRequestCultureProvider
        {
            CookieName = "glowbook_culture"
        });
});

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", p => p.RequireRole("Admin"));
    options.AddPolicy("CanManageAppointments", p => p.RequireClaim("permission", "manage_appointments"));
    options.AddPolicy("CanViewReports", p => p.RequireClaim("permission", "view_reports"));
});

// Extra middleware
builder.Services.AddSingleton<CultureCookieMiddleware>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddSession();

var app = builder.Build();

// DB migratie plus seed
using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    ctx.Database.Migrate();

    var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    string[] roles = { "Admin", "Employee", "Owner" };
    foreach (var role in roles)
    {
        if (!await roleMgr.RoleExistsAsync(role))
            await roleMgr.CreateAsync(new IdentityRole(role));
    }

    var adminEmail = "admin@glowbook.local";
    var admin = await userMgr.FindByEmailAsync(adminEmail);
    if (admin == null)
    {
        admin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            DisplayName = "Admin",
            EmailConfirmed = true,
            IsActive = true
        };

        var result = await userMgr.CreateAsync(admin, "Admin123!");
        if (result.Succeeded)
        {
            await userMgr.AddToRoleAsync(admin, "Admin");
            await userMgr.AddClaimAsync(admin, new System.Security.Claims.Claim("permission", "manage_appointments"));
            await userMgr.AddClaimAsync(admin, new System.Security.Claims.Claim("permission", "view_reports"));
        }
    }

    if (!ctx.Staff.Any())
    {
        ctx.Staff.AddRange(
            new Staff
            {
                Name = "Tamara",
                Email = "tamara@glowbook.local"
            },
            new Staff
            {
                Name = "Mila",
                Email = "mila@glowbook.local"
            }
        );

        await ctx.SaveChangesAsync();
    }
}

// Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseRequestLocalization();
app.UseMiddleware<CultureCookieMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllers(); // API

app.Run();
