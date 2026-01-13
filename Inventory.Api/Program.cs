using Inventory.Api.Extensions;
using Inventory.Api.Middleware;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Inventory.Infrastructure;
using Inventory.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// =======================
// Configuration
// =======================
var configuration = builder.Configuration;
var jwtKey = configuration["Jwt:Key"] ?? "YourSuperSecretKeyHere";
var jwtIssuer = configuration["Jwt:Issuer"] ?? "InventoryApi";
var jwtAudience = configuration["Jwt:Audience"] ?? "InventoryApi";

// =======================
// Services (DI)
// =======================
builder.Services.AddApplication();
builder.Services.AddInfrastructure(configuration); // ✅ Infrastructure handles DbContext, JWT, Repositories, Cache

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// =======================
// JWT Authentication
// =======================
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddLogging();
builder.Services.AddMemoryCache();

var app = builder.Build();

// =======================
// Database Migration + Seed
// =======================
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    await context.Database.MigrateAsync();

    if (!await context.Categories.AnyAsync())
    {
        await context.Categories.AddRangeAsync(
            new Category { Name = "Electronics" },
            new Category { Name = "Books" },
            new Category { Name = "Clothing" }
        );
    }

    if (!await context.Users.AnyAsync())
    {
        await context.Users.AddAsync(new User
        {
            Username = "admin",
            PasswordHash = HashPassword("Admin@123"),
            Role = UserRole.Admin
        });
    }

    await context.SaveChangesAsync();
}

// =======================
// Middleware Pipeline
// =======================
app.UseMiddleware<ExceptionMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// =======================
// Helper: Hash Password
// =======================
static string HashPassword(string password)
{
    using var sha256 = SHA256.Create();
    var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
    return Convert.ToBase64String(bytes);
}
