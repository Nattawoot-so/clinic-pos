namespace ClinicPOS.Api.Services;

public interface ITenantContext
{
    Guid TenantId { get; }
    Guid UserId { get; }
    string Role { get; }
}
