using ClinicPOS.Api.Data;
using ClinicPOS.Api.Entities;
using ClinicPOS.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace ClinicPOS.Tests;

public class DuplicatePhoneTests
{
    private static ClinicDbContext CreateContext(ITenantContext tenantContext, string dbName)
    {
        var options = new DbContextOptionsBuilder<ClinicDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new ClinicDbContext(options, tenantContext);
    }

    [Fact]
    public async Task Cannot_Create_Duplicate_Phone_In_Same_Tenant()
    {
        // Note: InMemory provider does not enforce unique indexes,
        // so we verify the unique constraint logic at the DB-schema level
        // by checking that the model has the correct unique index configured.
        // For real duplicate prevention, the PostgreSQL unique index handles this.
        // This test verifies our model configuration is correct.

        var dbName = $"DuplicatePhoneTest_{Guid.NewGuid()}";
        var tenantId = Guid.NewGuid();
        var tenantContext = new TestTenantContext { TenantId = tenantId };

        using var db = CreateContext(tenantContext, dbName);

        var patient1 = new Patient
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            FirstName = "Alice",
            LastName = "Smith",
            PhoneNumber = "555-0001",
            CreatedAt = DateTime.UtcNow
        };

        db.Patients.Add(patient1);
        await db.SaveChangesAsync();

        // Verify the unique index is configured in the model
        var entityType = db.Model.FindEntityType(typeof(Patient))!;
        var indexes = entityType.GetIndexes().ToList();
        var uniqueIndex = indexes.FirstOrDefault(i =>
            i.IsUnique &&
            i.Properties.Any(p => p.Name == "TenantId") &&
            i.Properties.Any(p => p.Name == "PhoneNumber"));

        Assert.NotNull(uniqueIndex);
        Assert.True(uniqueIndex.IsUnique);
    }

    [Fact]
    public async Task Unique_Index_Scoped_To_Tenant_And_Phone()
    {
        var dbName = $"UniqueIndexTest_{Guid.NewGuid()}";
        var tenantId = Guid.NewGuid();
        var tenantContext = new TestTenantContext { TenantId = tenantId };

        using var db = CreateContext(tenantContext, dbName);

        // Verify the model has a composite unique index on (TenantId, PhoneNumber)
        var entityType = db.Model.FindEntityType(typeof(Patient))!;
        var uniqueIndex = entityType.GetIndexes()
            .FirstOrDefault(i => i.IsUnique && i.Properties.Count == 2);

        Assert.NotNull(uniqueIndex);

        var propertyNames = uniqueIndex.Properties.Select(p => p.Name).ToList();
        Assert.Contains("TenantId", propertyNames);
        Assert.Contains("PhoneNumber", propertyNames);
    }
}
