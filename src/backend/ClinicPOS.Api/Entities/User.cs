namespace ClinicPOS.Api.Entities;

public class User : ITenantScoped
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "Viewer";
    public Tenant Tenant { get; set; } = null!;
    public List<UserBranch> UserBranches { get; set; } = new();
}
