using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Inventory.Infrastructure.Data;
using Inventory.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

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
        var repo = new ProductRepository(context);

        var product = new Product
        {
            Name = "Laptop",
            Description = "High-end gaming laptop", 
            Price = 100,
            StockQuantity = 10,                   
            Status = ProductStatus.Available,    
            CategoryId = 1                        
        };

        await repo.AddAsync(product);
        var saved = await context.Products.FirstOrDefaultAsync();

        Assert.NotNull(saved);
        Assert.Equal("Laptop", saved!.Name);
        Assert.Equal("High-end gaming laptop", saved.Description);
        Assert.Equal(100, saved.Price);
        Assert.Equal(10, saved.StockQuantity);
    }
}
