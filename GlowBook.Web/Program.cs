using GlowBook.Model.Data;
using GlowBook.Model.Entities;
using GlowBook.Web;
using GlowBook.Web.Middleware;
using GlowBook.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Globalization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);

// Serilog configuratie
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.File("logs/glowbook-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("DefaultConnection ontbreekt"),
        sql => sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)
    ));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.SignIn.RequireConfirmedEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// JWT (voor MAUI / API)
builder.Services.AddAuthentication()
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        var key = builder.Configuration["Jwt:Key"]
                  ?? throw new InvalidOperationException("Jwt:Key ontbreekt (User Secrets of appsettings).");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });


// Localisation
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.AddControllersWithViews()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    })
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();


// Meertaligheid (NL/EN)
var supportedCultures = new[]
{
    new CultureInfo("nl"),
    new CultureInfo("en"),
    new CultureInfo("fr")
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

// Email services (SMTP)
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));
builder.Services.AddSingleton(sp =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var settings = new EmailSettings();
    cfg.GetSection("Email").Bind(settings);
    return settings;
});
builder.Services.AddScoped<IEmailService, SmtpEmailService>();

// Middleware registreren als services
builder.Services.AddSingleton<CultureCookieMiddleware>();
builder.Services.AddSingleton<ErrorHandlingMiddleware>();
builder.Services.AddScoped<ActiveUserMiddleware>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

var app = builder.Build();

// Pipeline: ErrorHandlingMiddleware
app.UseMiddleware<ErrorHandlingMiddleware>();

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
            new Staff { Name = "Tamara", Email = "tamara@glowbook.local" },
            new Staff { Name = "Mila", Email = "mila@glowbook.local" }
        );

        await ctx.SaveChangesAsync();
    }
}

// Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseRouting();

app.UseRequestLocalization();
app.UseMiddleware<CultureCookieMiddleware>();

app.UseSession();
app.UseAuthentication();
app.UseMiddleware<ActiveUserMiddleware>();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllers(); // API

app.Run();
