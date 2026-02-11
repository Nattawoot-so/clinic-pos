namespace ClinicPOS.Api.Entities;

public interface ITenantScoped
{
    Guid TenantId { get; set; }
}
