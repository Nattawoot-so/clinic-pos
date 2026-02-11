using ClinicPOS.Api.Data;
using ClinicPOS.Api.Entities;
using ClinicPOS.Api.Messaging;
using ClinicPOS.Api.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ClinicPOS.Api.Features.Appointments;

public record CreateAppointmentRequest(Guid PatientId, Guid BranchId, DateTime StartAt);
public record AppointmentResponse(Guid Id, Guid PatientId, Guid BranchId, DateTime StartAt, DateTime CreatedAt);

public static class AppointmentEndpoints
{
    public static void MapAppointmentEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/appointments")
            .RequireAuthorization();

        group.MapPost("/", async (
            CreateAppointmentRequest req,
            ClinicDbContext db,
            ITenantContext tenant,
            IPublishEndpoint publishEndpoint) =>
        {
            if (tenant.Role == "Viewer")
                return Results.Json(new { error = "Viewers cannot create appointments" }, statusCode: 403);

            var appointment = new Appointment
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.TenantId,
                PatientId = req.PatientId,
                BranchId = req.BranchId,
                StartAt = req.StartAt,
                CreatedAt = DateTime.UtcNow
            };

            db.Appointments.Add(appointment);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: "23505" })
            {
                return Results.Json(
                    new { error = "Duplicate appointment: same patient, branch, and time already exists." },
                    statusCode: 409);
            }

            await publishEndpoint.Publish(new AppointmentCreatedEvent
            {
                AppointmentId = appointment.Id,
                TenantId = appointment.TenantId,
                PatientId = appointment.PatientId,
                BranchId = appointment.BranchId,
                StartAt = appointment.StartAt,
                CreatedAt = appointment.CreatedAt
            });

            return Results.Created($"/api/appointments/{appointment.Id}",
                new AppointmentResponse(appointment.Id, appointment.PatientId,
                    appointment.BranchId, appointment.StartAt, appointment.CreatedAt));
        });

        group.MapGet("/", async (Guid? branchId, ClinicDbContext db) =>
        {
            var query = db.Appointments.AsQueryable();

            if (branchId.HasValue)
                query = query.Where(a => a.BranchId == branchId.Value);

            var appointments = await query
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new AppointmentResponse(
                    a.Id, a.PatientId, a.BranchId, a.StartAt, a.CreatedAt))
                .ToListAsync();

            return Results.Ok(appointments);
        });
    }
}
