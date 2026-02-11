using ClinicPOS.Api.Entities;
using ClinicPOS.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace ClinicPOS.Api.Data;

public class ClinicDbContext : DbContext
{
    private readonly Guid _tenantId;

    public ClinicDbContext(DbContextOptions<ClinicDbContext> options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantId = tenantContext.TenantId;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserBranch> UserBranches => Set<UserBranch>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Appointment> Appointments => Set<Appointment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Composite key for UserBranch
        modelBuilder.Entity<UserBranch>()
            .HasKey(ub => new { ub.UserId, ub.BranchId });

        // Unique constraints
        modelBuilder.Entity<Patient>()
            .HasIndex(p => new { p.TenantId, p.PhoneNumber })
            .IsUnique();

        modelBuilder.Entity<Appointment>()
            .HasIndex(a => new { a.TenantId, a.PatientId, a.BranchId, a.StartAt })
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => new { u.TenantId, u.Username })
            .IsUnique();

        // Global query filters for tenant isolation
        modelBuilder.Entity<Patient>()
            .HasQueryFilter(p => p.TenantId == _tenantId);

        modelBuilder.Entity<Appointment>()
            .HasQueryFilter(a => a.TenantId == _tenantId);

        modelBuilder.Entity<Branch>()
            .HasQueryFilter(b => b.TenantId == _tenantId);

        modelBuilder.Entity<User>()
            .HasQueryFilter(u => u.TenantId == _tenantId);
    }
}
