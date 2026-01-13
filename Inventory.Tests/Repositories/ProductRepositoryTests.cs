using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Inventory.Infrastructure.Data;
using Inventory.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System;
using System.Threading.Tasks;

namespace Inventory.Tests.Repositories
{
    public class ProductRepositoryTests
    {
        private readonly DbContextOptions<AppDbContext> _options;

        public ProductRepositoryTests()
        {
            _options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // unique DB for each test
                .Options;
        }

        [Fact]
        public async Task AddAsync_AddsProduct()
        {
            await using var context = new AppDbContext(_options);

            // Arrange: add a category first
            var category = new Category { Id = 1, Name = "Electronics" };
            await context.Categories.AddAsync(category);
            await context.SaveChangesAsync();

            var repo = new ProductRepository(context);

            var product = new Product
            {
                Name = "Laptop",
                Description = "High-end gaming laptop",
                Price = 100m,           // decimal literal
                StockQuantity = 10,
                Status = ProductStatus.Available,
                CategoryId = category.Id
            };

            // Act
            await repo.AddAsync(product);
            var saved = await context.Products.Include(p => p.Category).FirstOrDefaultAsync();

            // Assert
            Assert.NotNull(saved);
            Assert.Equal("Laptop", saved!.Name);
            Assert.Equal("High-end gaming laptop", saved.Description);
            Assert.Equal(100m, saved.Price);
            Assert.Equal(10, saved.StockQuantity);
            Assert.NotNull(saved.Category);
            Assert.Equal("Electronics", saved.Category!.Name);
        }
    }
}
