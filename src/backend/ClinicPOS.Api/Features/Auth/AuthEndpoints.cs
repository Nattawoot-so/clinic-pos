using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ClinicPOS.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ClinicPOS.Api.Features.Auth;

public record LoginRequest(string Username, string Password);
public record LoginResponse(string Token);

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        app.MapPost("/api/auth/login", async (LoginRequest req, ClinicDbContext db, IConfiguration config) =>
        {
            var user = await db.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Username == req.Username);

            if (user is null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
                return Results.Json(new { error = "Invalid username or password" }, statusCode: 401);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
            var claims = new[]
            {
                new Claim("user_id", user.Id.ToString()),
                new Claim("tenant_id", user.TenantId.ToString()),
                new Claim("role", user.Role),
                new Claim("username", user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

            return Results.Ok(new LoginResponse(new JwtSecurityTokenHandler().WriteToken(token)));
        });
    }
}
