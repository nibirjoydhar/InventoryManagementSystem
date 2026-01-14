using Inventory.Application.Interfaces;
using Inventory.Domain.Interfaces;
using Inventory.Infrastructure.Data;
using Inventory.Infrastructure.Repositories;
using Inventory.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Inventory.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // =====================
            // Database
            // =====================
            var connectionString = configuration.GetConnectionString("DefaultConnection")!;
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString));

            // =====================
            // Repositories
            // =====================
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IUserRepository, UserRepository>();

            // =====================
            // JWT Service
            // =====================
            var key = configuration.GetValue<string>("Jwt:Key") ?? "YourSuperSecretKeyHere";
            services.AddScoped<IJwtService>(_ => new JwtService(key));

            // =====================
            // Cache
            // =====================
            services.AddMemoryCache();
            services.AddSingleton<ICacheService, MemoryCacheService>();

            return services;
        }
    }
}
