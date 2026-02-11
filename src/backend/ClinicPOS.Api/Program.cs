using System.Text;
using ClinicPOS.Api.Data;
using ClinicPOS.Api.Features.Appointments;
using ClinicPOS.Api.Features.Auth;
using ClinicPOS.Api.Features.Patients;
using ClinicPOS.Api.Features.Users;
using ClinicPOS.Api.Messaging;
using ClinicPOS.Api.Seeding;
using ClinicPOS.Api.Services;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// EF Core + PostgreSQL
builder.Services.AddDbContext<ClinicDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Tenant context
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantContext, HttpTenantContext>();

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = false,
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });
builder.Services.AddAuthorization();

// MassTransit + RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<AppointmentCreatedConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"] ?? "localhost");
        cfg.ConfigureEndpoints(context);
    });
});

// CORS
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

// Seeder
builder.Services.AddScoped<DataSeeder>();

var app = builder.Build();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// Map endpoints
app.MapAuthEndpoints();
app.MapPatientEndpoints();
app.MapUserEndpoints();
app.MapAppointmentEndpoints();

// Branches endpoint
app.MapGet("/api/branches", async (ClinicDbContext db) =>
    Results.Ok(await db.Branches.Select(b => new { b.Id, b.Name }).ToListAsync()))
    .RequireAuthorization();

// Health check
app.MapGet("/api/health", () => Results.Ok(new { status = "healthy" }));

// Auto-migrate and seed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ClinicDbContext>();
    if (db.Database.IsRelational())
    {
        await db.Database.MigrateAsync();
    }
    else
    {
        await db.Database.EnsureCreatedAsync();
    }
    await scope.ServiceProvider.GetRequiredService<DataSeeder>().SeedAsync();
}

app.Run();

public partial class Program { }
