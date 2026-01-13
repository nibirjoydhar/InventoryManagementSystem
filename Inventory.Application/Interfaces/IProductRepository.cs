using Inventory.Domain.Entities;
using Inventory.Domain.Interfaces;
using Inventory.Application.DTOs.Product;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Inventory.Application.Interfaces
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<IReadOnlyList<Product>> GetAllAsync(ProductQueryParamsDto queryParams);
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId);
    }
}
