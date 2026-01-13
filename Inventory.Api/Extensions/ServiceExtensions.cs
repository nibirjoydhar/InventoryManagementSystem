using Inventory.Application.Interfaces;
using Inventory.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Inventory.Api.Extensions
{
    /// <summary>
    /// Extension methods for registering application layer services
    /// </summary>
    public static class ServiceExtensions
    {
        /// <summary>
        /// Registers all application layer services and AutoMapper profiles
        /// </summary>
        /// <param name="services">The IServiceCollection to add services to</param>
        /// <returns>The updated IServiceCollection</returns>
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IAuthService, AuthService>();

            // AutoMapper configuration
            services.AddAutoMapper(cfg => { },
                typeof(ProductService).Assembly,
                typeof(CategoryService).Assembly,
                typeof(AuthService).Assembly);

            return services;
        }
    }
}
