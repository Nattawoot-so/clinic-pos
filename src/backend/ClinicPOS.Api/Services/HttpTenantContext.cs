namespace ClinicPOS.Api.Services;

public class HttpTenantContext : ITenantContext
{
    private readonly IHttpContextAccessor _accessor;

    public HttpTenantContext(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public Guid TenantId =>
        Guid.TryParse(_accessor.HttpContext?.User.FindFirst("tenant_id")?.Value, out var id)
            ? id : Guid.Empty;

    public Guid UserId =>
        Guid.TryParse(_accessor.HttpContext?.User.FindFirst("user_id")?.Value, out var id)
            ? id : Guid.Empty;

    public string Role =>
        _accessor.HttpContext?.User.FindFirst("role")?.Value ?? string.Empty;
}
