namespace ClinicPOS.Api.Messaging;

public record AppointmentCreatedEvent
{
    public Guid AppointmentId { get; init; }
    public Guid TenantId { get; init; }
    public Guid PatientId { get; init; }
    public Guid BranchId { get; init; }
    public DateTime StartAt { get; init; }
    public DateTime CreatedAt { get; init; }
}
