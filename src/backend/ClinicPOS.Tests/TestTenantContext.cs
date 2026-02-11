using ClinicPOS.Api.Services;

namespace ClinicPOS.Tests;

public class TestTenantContext : ITenantContext
{
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public string Role { get; set; } = "Admin";
}
