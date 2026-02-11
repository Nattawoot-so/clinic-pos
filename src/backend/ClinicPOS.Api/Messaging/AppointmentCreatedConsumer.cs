using MassTransit;

namespace ClinicPOS.Api.Messaging;

public class AppointmentCreatedConsumer : IConsumer<AppointmentCreatedEvent>
{
    private readonly ILogger<AppointmentCreatedConsumer> _logger;

    public AppointmentCreatedConsumer(ILogger<AppointmentCreatedConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<AppointmentCreatedEvent> context)
    {
        var msg = context.Message;
        _logger.LogInformation(
            "[RabbitMQ] AppointmentCreated — AppointmentId={AppointmentId}, TenantId={TenantId}, PatientId={PatientId}, BranchId={BranchId}, StartAt={StartAt}",
            msg.AppointmentId, msg.TenantId, msg.PatientId, msg.BranchId, msg.StartAt);
        return Task.CompletedTask;
    }
}
