using ClinicPOS.Api.Data;
using ClinicPOS.Api.Entities;
using ClinicPOS.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace ClinicPOS.Api.Features.Users;

public record CreateUserRequest(string Username, string Password, string Role);
public record AssignRoleRequest(string Role);
public record AssociateBranchRequest(Guid BranchId);
public record UserResponse(Guid Id, string Username, string Role, Guid TenantId);

public static class UserEndpoints
{
    private static readonly string[] ValidRoles = ["Admin", "User", "Viewer"];

    public static void MapUserEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/users")
            .RequireAuthorization();

        group.MapPost("/", async (CreateUserRequest req, ClinicDbContext db, ITenantContext tenant) =>
        {
            if (tenant.Role != "Admin")
                return Results.Json(new { error = "Only admins can create users" }, statusCode: 403);

            if (!ValidRoles.Contains(req.Role))
                return Results.Json(new { error = $"Role must be one of: {string.Join(", ", ValidRoles)}" }, statusCode: 400);

            var user = new User
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.TenantId,
                Username = req.Username.Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
                Role = req.Role
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();

            return Results.Created($"/api/users/{user.Id}",
                new UserResponse(user.Id, user.Username, user.Role, user.TenantId));
        });

        group.MapPut("/{id:guid}/role", async (Guid id, AssignRoleRequest req, ClinicDbContext db, ITenantContext tenant) =>
        {
            if (tenant.Role != "Admin")
                return Results.Json(new { error = "Only admins can assign roles" }, statusCode: 403);

            if (!ValidRoles.Contains(req.Role))
                return Results.Json(new { error = $"Role must be one of: {string.Join(", ", ValidRoles)}" }, statusCode: 400);

            var user = await db.Users.FindAsync(id);
            if (user is null) return Results.NotFound();

            user.Role = req.Role;
            await db.SaveChangesAsync();

            return Results.Ok(new UserResponse(user.Id, user.Username, user.Role, user.TenantId));
        });

        group.MapPost("/{id:guid}/branches", async (Guid id, AssociateBranchRequest req, ClinicDbContext db, ITenantContext tenant) =>
        {
            if (tenant.Role != "Admin")
                return Results.Json(new { error = "Only admins can associate branches" }, statusCode: 403);

            var user = await db.Users.FindAsync(id);
            if (user is null) return Results.NotFound();

            var branch = await db.Branches.FindAsync(req.BranchId);
            if (branch is null) return Results.Json(new { error = "Branch not found" }, statusCode: 404);

            db.UserBranches.Add(new UserBranch { UserId = id, BranchId = req.BranchId });
            await db.SaveChangesAsync();

            return Results.Ok(new { message = "Branch associated successfully" });
        });

        group.MapGet("/", async (ClinicDbContext db) =>
        {
            var users = await db.Users
                .Select(u => new UserResponse(u.Id, u.Username, u.Role, u.TenantId))
                .ToListAsync();

            return Results.Ok(users);
        });
    }
}
