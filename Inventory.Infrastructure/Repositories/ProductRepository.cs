using Inventory.Application.DTOs.Product;
using Inventory.Domain.Entities;
using Inventory.Domain.Interfaces;
using Inventory.Application.Interfaces;
using Inventory.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Inventory.Infrastructure.Repositories
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        public ProductRepository(AppDbContext context) : base(context)
        {
        }

        // ============================
        // Filtered GetAllAsync using DTO
        // ============================
        public async Task<(int Total, IReadOnlyList<Product> Items)> GetAllWithTotalAsync(ProductQueryParamsDto queryParams)
        {
            var query = _context.Products
                .Include(p => p.Category)           // Important for CategoryName
                .AsNoTracking();

            // Apply filters
            if (queryParams.MinPrice.HasValue)
                query = query.Where(p => p.Price >= queryParams.MinPrice.Value);

            if (queryParams.MaxPrice.HasValue)
                query = query.Where(p => p.Price <= queryParams.MaxPrice.Value);

            if (queryParams.CategoryId.HasValue)
                query = query.Where(p => p.CategoryId == queryParams.CategoryId.Value);

            // Sorting (handle null/default)
            var sortBy = string.IsNullOrEmpty(queryParams.SortBy) ? "id" : queryParams.SortBy.ToLower();

            query = sortBy switch
            {
                "name" => queryParams.Ascending
                    ? query.OrderBy(p => p.Name)
                    : query.OrderByDescending(p => p.Name),
                "price" => queryParams.Ascending
                    ? query.OrderBy(p => p.Price)
                    : query.OrderByDescending(p => p.Price),
                _ => queryParams.Ascending
                    ? query.OrderBy(p => p.Id)
                    : query.OrderByDescending(p => p.Id)
            };

            // Get total count (before pagination - very efficient)
            var total = await query.CountAsync();

            // Get paginated items
            var items = await query
                .Skip((queryParams.Page - 1) * queryParams.PageSize)
                .Take(queryParams.PageSize)
                .ToListAsync();

            return (total, items);
        }

        // ============================
        // Get products by category
        // ============================
        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId)
        {
            return await _context.Products
                .Where(p => p.CategoryId == categoryId)
                .Include(p => p.Category)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
