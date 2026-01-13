using Inventory.Application.Interfaces;
using Inventory.Application.Services;
using Inventory.Domain.Interfaces;
using Inventory.Infrastructure.Authentication;     // assuming correct JwtService is here
using Inventory.Infrastructure.Data;
using Inventory.Infrastructure.Repositories;
using Inventory.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Inventory.Api.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString));

            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IUserRepository, UserRepository>();

            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IAuthService, AuthService>();

            services.AddMemoryCache();
            services.AddScoped<ICacheService, MemoryCacheService>();

            return services;
        }

        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IJwtService>(provider =>
            {
                var config = provider.GetRequiredService<IConfiguration>();
                var key = config["Jwt:Key"] ?? "YourSuperSecretKeyHere";
                var issuer = config["Jwt:Issuer"] ?? "InventoryApi";
                return new Inventory.Infrastructure.Authentication.JwtService(key, issuer);
            });

            // Fixed AutoMapper registration for version 13+
            services.AddAutoMapper(
                cfg => { },   // ← required empty action
                typeof(ProductService).Assembly
            );

            return services;
        }
    }
}