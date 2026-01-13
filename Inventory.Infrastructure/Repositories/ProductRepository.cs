using Inventory.Domain.Entities;
using Inventory.Domain.Interfaces;
using Inventory.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;

        public ProductRepository(AppDbContext context)
        {
            _context = context;
        }

        // ============================
        // Add a new product
        // ============================
        public async Task AddAsync(Product product)
        {
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
        }

        // ============================
        // Delete product by Id
        // ============================
        public async Task DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return;

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }

        // ============================
        // Get all products (read-only)
        // ============================
        public async Task<IReadOnlyList<Product>> GetAllAsync()
        {
            return await _context.Products
                                 .Include(p => p.Category) // eager load category
                                 .AsNoTracking()           // read-only optimization
                                 .ToListAsync();
        }

        // ============================
        // Get product by Id
        // ============================
        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _context.Products
                                 .Include(p => p.Category)
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync(p => p.Id == id);
        }

        // ============================
        // Update product
        // ============================
        public async Task UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        // ============================
        // Get products by category (read-only)
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
