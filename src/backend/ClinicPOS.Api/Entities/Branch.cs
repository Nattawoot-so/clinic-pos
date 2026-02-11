namespace ClinicPOS.Api.Entities;

public class Branch : ITenantScoped
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public Tenant Tenant { get; set; } = null!;
}
