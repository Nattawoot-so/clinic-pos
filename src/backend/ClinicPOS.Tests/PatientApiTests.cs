using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using ClinicPOS.Api.Data;
using ClinicPOS.Api.Services;
using MassTransit;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace ClinicPOS.Tests;

public class PatientApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Guid _tenantId = Guid.NewGuid();

    private readonly string _dbName = $"TestDb_{Guid.NewGuid()}";

    public PatientApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove ALL DbContext-related registrations
                var dbContextDescriptors = services
                    .Where(d => d.ServiceType == typeof(DbContextOptions<ClinicDbContext>)
                             || d.ServiceType == typeof(DbContextOptions)
                             || d.ServiceType.FullName?.Contains("EntityFrameworkCore") == true)
                    .ToList();
                foreach (var d in dbContextDescriptors)
                    services.Remove(d);

                // Re-add with InMemory provider only (same DB name for all scopes)
                var dbName = _dbName;
                services.AddDbContext<ClinicDbContext>(options =>
                    options.UseInMemoryDatabase(dbName));

                // Replace tenant context
                var tenantDescriptors = services
                    .Where(d => d.ServiceType == typeof(ITenantContext))
                    .ToList();
                foreach (var d in tenantDescriptors)
                    services.Remove(d);

                var testTenantContext = new TestTenantContext
                {
                    TenantId = _tenantId,
                    UserId = Guid.NewGuid(),
                    Role = "Admin"
                };
                services.AddSingleton<ITenantContext>(testTenantContext);

                // Replace MassTransit with test harness (no RabbitMQ)
                services.AddMassTransitTestHarness();
            });
        });
    }

    private string GenerateTestToken()
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes("super-secret-key-at-least-32-chars-long-for-hmac!!"));
        var claims = new[]
        {
            new Claim("user_id", Guid.NewGuid().ToString()),
            new Claim("tenant_id", _tenantId.ToString()),
            new Claim("role", "Admin"),
            new Claim(ClaimTypes.Role, "Admin")
        };

        var token = new JwtSecurityToken(
            issuer: "ClinicPOS",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [Fact]
    public async Task Create_And_List_Patients_Integration()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", GenerateTestToken());

        // Create patient
        var createResponse = await client.PostAsJsonAsync("/api/patients", new
        {
            firstName = "Test",
            lastName = "Patient",
            phoneNumber = "999-0001"
        });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        // List patients
        var listResponse = await client.GetAsync("/api/patients");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        var patients = await listResponse.Content.ReadFromJsonAsync<List<PatientDto>>();
        Assert.NotNull(patients);
        Assert.Contains(patients, p => p.FirstName == "Test" && p.LastName == "Patient");
    }

    private record PatientDto(Guid Id, string FirstName, string LastName, string PhoneNumber, Guid? PrimaryBranchId, DateTime CreatedAt);
}
