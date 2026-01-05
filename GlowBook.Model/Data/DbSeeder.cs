using GlowBook.Model.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GlowBook.Model.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(
        AppDbContext ctx,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        // Rollen
        string[] roles = { "Admin", "Owner", "Employee" };
        foreach (var r in roles)
        {
            if (!await roleManager.RoleExistsAsync(r))
                await roleManager.CreateAsync(new IdentityRole(r));
        }

        // Admin
        var adminEmail = "admin@glowbook.local";
        var admin = await userManager.FindByEmailAsync(adminEmail);
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

            var result = await userManager.CreateAsync(admin, "Admin123!");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, "Admin");
        }

        // Staff 
        if (!await ctx.Staff.AnyAsync())
        {
            ctx.Staff.AddRange(
                new Staff { Name = "Tamara", Email = "tamara@glowbook.local" },
                new Staff { Name = "Mila", Email = "mila@glowbook.local" }
            );
        }

        // Customers
        if (!await ctx.Customers.AnyAsync())
        {
            ctx.Customers.AddRange(
                new Customer { Name = "Lien", Email = "lien@test.be", Phone = "0499 11 22 33" },
                new Customer { Name = "Sara", Email = "sara@test.be", Phone = "0488 44 55 66" }
            );
        }

        // Services
        if (!await ctx.Services.AnyAsync())
        {
            ctx.Services.AddRange(
                new Service { Name = "Gezichtsbehandeling", DurationMinutes = 60, Price = 95 },
                new Service { Name = "Manicure", DurationMinutes = 30, Price = 55 }
            );
        }

        await ctx.SaveChangesAsync();

        // Appointments + AppointmentServices
        if (!await ctx.Appointments.AnyAsync())
        {
            var cust = await ctx.Customers.FirstAsync();
            var service = await ctx.Services.FirstAsync();
            var staff = await ctx.Staff.FirstAsync(); 

            var start = DateTime.Today.AddDays(1).AddHours(9);
            var end = start.AddMinutes(60);

            var appt = new Appointment
            {
                CustomerId = cust.Id,
                StaffId = staff.Id,
                Start = start,
                End = end,
                Status = "Ingepland"
            };

            ctx.Appointments.Add(appt);
            await ctx.SaveChangesAsync();

            ctx.AppointmentServices.Add(new AppointmentService
            {
                AppointmentId = appt.Id,
                ServiceId = service.Id
            });

            await ctx.SaveChangesAsync();
        }
    }
}
