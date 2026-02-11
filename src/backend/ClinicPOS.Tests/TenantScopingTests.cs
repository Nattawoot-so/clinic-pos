using ClinicPOS.Api.Data;
using ClinicPOS.Api.Entities;
using ClinicPOS.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicPOS.Tests;

public class TenantScopingTests
{
    private static ClinicDbContext CreateContext(ITenantContext tenantContext, string dbName)
    {
        var options = new DbContextOptionsBuilder<ClinicDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new ClinicDbContext(options, tenantContext);
    }

    [Fact]
    public async Task Patients_Are_Isolated_By_Tenant()
    {
        var dbName = $"TenantScopingTest_{Guid.NewGuid()}";
        var tenantAId = Guid.NewGuid();
        var tenantBId = Guid.NewGuid();

        // Seed data using Tenant A context
        var tenantAContext = new TestTenantContext { TenantId = tenantAId };
        using (var db = CreateContext(tenantAContext, dbName))
        {
            db.Patients.Add(new Patient
            {
                Id = Guid.NewGuid(),
                TenantId = tenantAId,
                FirstName = "Alice",
                LastName = "Smith",
                PhoneNumber = "111-1111",
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        // Seed data using Tenant B context
        var tenantBContext = new TestTenantContext { TenantId = tenantBId };
        using (var db = CreateContext(tenantBContext, dbName))
        {
            db.Patients.Add(new Patient
            {
                Id = Guid.NewGuid(),
                TenantId = tenantBId,
                FirstName = "Bob",
                LastName = "Jones",
                PhoneNumber = "222-2222",
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        // Query as Tenant A — should only see Alice
        using (var db = CreateContext(tenantAContext, dbName))
        {
            var patients = await db.Patients.ToListAsync();
            Assert.Single(patients);
            Assert.Equal("Alice", patients[0].FirstName);
        }

        // Query as Tenant B — should only see Bob
        using (var db = CreateContext(tenantBContext, dbName))
        {
            var patients = await db.Patients.ToListAsync();
            Assert.Single(patients);
            Assert.Equal("Bob", patients[0].FirstName);
        }
    }

    [Fact]
    public async Task Same_Phone_Allowed_Across_Different_Tenants()
    {
        var dbName = $"CrossTenantPhoneTest_{Guid.NewGuid()}";
        var tenantAId = Guid.NewGuid();
        var tenantBId = Guid.NewGuid();
        var sharedPhone = "555-0000";

        var tenantAContext = new TestTenantContext { TenantId = tenantAId };
        using (var db = CreateContext(tenantAContext, dbName))
        {
            db.Patients.Add(new Patient
            {
                Id = Guid.NewGuid(),
                TenantId = tenantAId,
                FirstName = "Alice",
                LastName = "A",
                PhoneNumber = sharedPhone,
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        // Same phone in different tenant should succeed
        var tenantBContext = new TestTenantContext { TenantId = tenantBId };
        using (var db = CreateContext(tenantBContext, dbName))
        {
            db.Patients.Add(new Patient
            {
                Id = Guid.NewGuid(),
                TenantId = tenantBId,
                FirstName = "Bob",
                LastName = "B",
                PhoneNumber = sharedPhone,
                CreatedAt = DateTime.UtcNow
            });
            // Should NOT throw — different tenants
            await db.SaveChangesAsync();
        }

        // Verify both exist
        using (var db = CreateContext(tenantAContext, dbName))
        {
            var patients = await db.Patients.ToListAsync();
            Assert.Single(patients);
            Assert.Equal("Alice", patients[0].FirstName);
        }
    }
}
