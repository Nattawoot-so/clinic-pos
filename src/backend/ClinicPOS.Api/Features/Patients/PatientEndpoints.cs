using ClinicPOS.Api.Data;
using ClinicPOS.Api.Entities;
using ClinicPOS.Api.Services;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ClinicPOS.Api.Features.Patients;

public record CreatePatientRequest(string FirstName, string LastName, string PhoneNumber, Guid? PrimaryBranchId);
public record PatientResponse(Guid Id, string FirstName, string LastName, string PhoneNumber, Guid? PrimaryBranchId, DateTime CreatedAt);

public static class PatientEndpoints
{
    public static void MapPatientEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/patients")
            .RequireAuthorization();

        group.MapPost("/", async (CreatePatientRequest req, ClinicDbContext db, ITenantContext tenant) =>
        {
            if (tenant.Role == "Viewer")
                return Results.Json(new { error = "Viewers cannot create patients" }, statusCode: 403);

            if (string.IsNullOrWhiteSpace(req.FirstName))
                return Results.Json(new { error = "FirstName is required" }, statusCode: 400);
            if (string.IsNullOrWhiteSpace(req.LastName))
                return Results.Json(new { error = "LastName is required" }, statusCode: 400);
            if (string.IsNullOrWhiteSpace(req.PhoneNumber))
                return Results.Json(new { error = "PhoneNumber is required" }, statusCode: 400);

            var patient = new Patient
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.TenantId,
                FirstName = req.FirstName.Trim(),
                LastName = req.LastName.Trim(),
                PhoneNumber = req.PhoneNumber.Trim(),
                PrimaryBranchId = req.PrimaryBranchId,
                CreatedAt = DateTime.UtcNow
            };

            db.Patients.Add(patient);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: "23505" })
            {
                return Results.Json(
                    new { error = "A patient with this phone number already exists in this tenant." },
                    statusCode: 409);
            }

            return Results.Created($"/api/patients/{patient.Id}",
                new PatientResponse(patient.Id, patient.FirstName, patient.LastName,
                    patient.PhoneNumber, patient.PrimaryBranchId, patient.CreatedAt));
        });

        group.MapGet("/", async (Guid? branchId, ClinicDbContext db) =>
        {
            var query = db.Patients.AsQueryable();

            if (branchId.HasValue)
                query = query.Where(p => p.PrimaryBranchId == branchId.Value);

            var patients = await query
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PatientResponse(
                    p.Id, p.FirstName, p.LastName,
                    p.PhoneNumber, p.PrimaryBranchId, p.CreatedAt))
                .ToListAsync();

            return Results.Ok(patients);
        });

        group.MapGet("/{id:guid}", async (Guid id, ClinicDbContext db) =>
        {
            var patient = await db.Patients
                .Where(p => p.Id == id)
                .Select(p => new PatientResponse(
                    p.Id, p.FirstName, p.LastName,
                    p.PhoneNumber, p.PrimaryBranchId, p.CreatedAt))
                .FirstOrDefaultAsync();

            return patient is null ? Results.NotFound() : Results.Ok(patient);
        });
    }
}
