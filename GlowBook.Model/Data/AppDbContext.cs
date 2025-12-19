using GlowBook.Model.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GlowBook.Model.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Staff> Staff => Set<Staff>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<AppointmentService> AppointmentServices => Set<AppointmentService>();

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
        
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Soft delete filter
        builder.Entity<Customer>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Staff>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Service>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Appointment>().HasQueryFilter(e => !e.IsDeleted);

        // Many to many Appointment <-> Service
        builder.Entity<AppointmentService>()
            .HasKey(x => new { x.AppointmentId, x.ServiceId });

        builder.Entity<AppointmentService>()
            .HasOne(x => x.Appointment)
            .WithMany(a => a.AppointmentServices)
            .HasForeignKey(x => x.AppointmentId);

        builder.Entity<AppointmentService>()
            .HasOne(x => x.Service)
            .WithMany(s => s.AppointmentServices)
            .HasForeignKey(x => x.ServiceId);

        builder.Entity<Service>()
            .Property(s => s.Price)
            .HasPrecision(10, 2);

        // (Appointment heeft soft delete filter, join niet)
        builder.Entity<AppointmentService>()
            .HasQueryFilter(x => !x.Appointment.IsDeleted && !x.Service.IsDeleted);

    }

    public override int SaveChanges()
    {
        ApplySoftDelete();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplySoftDelete();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplySoftDelete()
    {
        var entries = ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.State == EntityState.Deleted);

        foreach (var entry in entries)
        {
            entry.State = EntityState.Modified;
            entry.Entity.IsDeleted = true;
        }
    }
}
