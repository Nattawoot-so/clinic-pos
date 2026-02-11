namespace ClinicPOS.Api.Entities;

public class Patient : ITenantScoped
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public Guid? PrimaryBranchId { get; set; }
    public DateTime CreatedAt { get; set; }
    public Tenant Tenant { get; set; } = null!;
    public Branch? PrimaryBranch { get; set; }
}
