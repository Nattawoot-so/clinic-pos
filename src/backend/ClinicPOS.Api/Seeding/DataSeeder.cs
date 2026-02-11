using ClinicPOS.Api.Data;
using ClinicPOS.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicPOS.Api.Seeding;

public class DataSeeder
{
    private readonly ClinicDbContext _db;

    public DataSeeder(ClinicDbContext db) => _db = db;

    public async Task SeedAsync()
    {
        if (await _db.Tenants.IgnoreQueryFilters().AnyAsync())
            return;

        var tenantId = Guid.NewGuid();
        var tenant = new Tenant { Id = tenantId, Name = "Downtown Clinic Group" };

        var branch1Id = Guid.NewGuid();
        var branch2Id = Guid.NewGuid();
        var branch1 = new Branch { Id = branch1Id, TenantId = tenantId, Name = "Main Street Branch" };
        var branch2 = new Branch { Id = branch2Id, TenantId = tenantId, Name = "Eastside Branch" };

        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Username = "admin",
            Role = "Admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123")
        };
        var normalUser = new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Username = "user",
            Role = "User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("user123")
        };
        var viewerUser = new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Username = "viewer",
            Role = "Viewer",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("viewer123")
        };

        _db.Tenants.Add(tenant);
        _db.Branches.AddRange(branch1, branch2);
        _db.Users.AddRange(adminUser, normalUser, viewerUser);

        _db.UserBranches.AddRange(
            new UserBranch { UserId = adminUser.Id, BranchId = branch1Id },
            new UserBranch { UserId = adminUser.Id, BranchId = branch2Id },
            new UserBranch { UserId = normalUser.Id, BranchId = branch1Id },
            new UserBranch { UserId = viewerUser.Id, BranchId = branch2Id }
        );

        await _db.SaveChangesAsync();
    }
}
