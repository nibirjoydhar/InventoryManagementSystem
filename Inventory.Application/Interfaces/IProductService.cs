using Inventory.Application.DTOs.Product;

namespace Inventory.Application.Interfaces
{
    public interface IProductService
    {
        Task<(int Total, IReadOnlyList<ProductDto> Items)> GetAllAsync(ProductQueryParamsDto? queryParams = null);
        Task<ProductDto?> GetByIdAsync(int id);
        Task<ProductDto> CreateAsync(CreateProductDto dto);
        Task<ProductDto?> UpdateAsync(int id, UpdateProductDto dto);
        Task<bool> DeleteAsync(int id);
    }
}