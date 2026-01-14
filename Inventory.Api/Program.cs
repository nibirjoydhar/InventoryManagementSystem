using Inventory.Api.Extensions;
using Inventory.Api.Middleware;
using Inventory.Infrastructure;
using Inventory.Infrastructure.Data;
using Inventory.Infrastructure.Data.Seed;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.OpenApi;
using System.Reflection;
using System.Text;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// =======================
// Configuration
// =======================
var configuration = builder.Configuration;
var jwtKey = configuration["Jwt:Key"] ?? "YourSuperSecretKeyHereMustBeAtLeast32CharsLong!";
var jwtIssuer = configuration["Jwt:Issuer"] ?? "InventoryApi";
var jwtAudience = configuration["Jwt:Audience"] ?? "InventoryApi";

// =======================
// Services (DI)
// =======================
builder.Services.AddApplication();
builder.Services.AddInfrastructure(configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger with JWT support
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Inventory Management API",
        Version = "v1"
    });

    // Include XML comments from API project
    var xmlApiFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlApiPath = Path.Combine(AppContext.BaseDirectory, xmlApiFile);
    options.IncludeXmlComments(xmlApiPath, includeControllerXmlComments: true);

    // Include XML comments from Application project
    var xmlAppFile = "Inventory.Application.xml"; // exact file name
    var xmlAppPath = Path.Combine(AppContext.BaseDirectory, xmlAppFile);
    if (File.Exists(xmlAppPath))
    {
        options.IncludeXmlComments(xmlAppPath);
    }

    const string schemeName = "bearer";

    options.AddSecurityDefinition(schemeName, new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Description = "Paste **only your JWT token** here."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference(schemeName, document)] = []
    });

    // Optional: Enable annotations from XML
    options.EnableAnnotations();
});


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
        ValidateAudience = false,
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

// Seed Product & Category
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    ProductCategorySeeder.Seed(dbContext);
}

// =======================
// Automatic migrations (good for dev, consider removing in production)
// =======================
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.Migrate();
}

// =======================
// Middleware pipeline
// =======================
app.UseMiddleware<ExceptionMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Inventory Management API v1");
    c.DefaultModelsExpandDepth(-1); // hides object schemas by default
    c.DisplayRequestDuration();      // shows request duration
});


app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();