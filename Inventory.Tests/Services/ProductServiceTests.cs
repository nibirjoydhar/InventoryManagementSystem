using AutoMapper;
using Inventory.Application.DTOs.Product;
using Inventory.Application.Services;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Inventory.Domain.Interfaces;
using Inventory.Infrastructure.Services;
using Moq;
using Xunit;
using FluentAssertions;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace Inventory.Tests.Services
{
    public class ProductServiceTests
    {
        private readonly Mock<IMapper> _mapperMock;
        private readonly IMapper _mapper;
        private readonly Mock<IProductRepository> _productRepoMock;
        private readonly MemoryCacheService _cache;
        private readonly ProductService _service;

        public ProductServiceTests()
        {
            _mapperMock = new Mock<IMapper>();
            _mapper = _mapperMock.Object;
            _productRepoMock = new Mock<IProductRepository>();
            _cache = new MemoryCacheService(new Microsoft.Extensions.Caching.Memory.MemoryCache(
                new Microsoft.Extensions.Caching.Memory.MemoryCacheOptions()));
            _service = new ProductService(_productRepoMock.Object, _mapper, _cache);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllProducts()
        {
            var products = new List<Product>
            {
                new Product
                {
                    Id = 1,
                    Name = "Test",
                    Description = "Test product",
                    Price = 10,
                    StockQuantity = 5,
                    CategoryId = 1,
                    Status = ProductStatus.Available,
                    Category = new Category { Id = 1, Name = "Electronics" }
                }
            };

            _productRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(products);

            var result = await _service.GetAllAsync();

            result.Should().HaveCount(1);
            result.First().Name.Should().Be("Test");
            result.First().CategoryName.Should().Be("Electronics");
        }

        [Fact]
        public async Task CreateAsync_AddsProduct()
        {
            var dto = new CreateProductDto
            {
                Name = "New",
                Description = "Test product",
                Price = 5,
                StockQuantity = 10,
                CategoryId = 1,
                Status = (int)ProductStatus.Available
            };

            // Simulate repository adding the product with a Category assigned
            Product addedProduct = new Product
            {
                Id = 1,
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                StockQuantity = dto.StockQuantity,
                Status = ProductStatus.Available,
                CategoryId = dto.CategoryId,
                Category = new Category { Id = 1, Name = "Electronics" } // avoid null
            };

            _productRepoMock.Setup(x => x.AddAsync(It.IsAny<Product>()))
                            .Callback<Product>(p => addedProduct = p)
                            .Returns(Task.CompletedTask);

            _productRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                            .ReturnsAsync(addedProduct);

            var result = await _service.CreateAsync(dto);

            result.Name.Should().Be(dto.Name);
            result.CategoryName.Should().Be("Electronics");
        }

        [Fact]
        public async Task CreateAsync_ReturnsProduct_WhenCategoryIsNull()
        {
            var dto = new CreateProductDto
            {
                Name = "NoCategory",
                Description = "No category product",
                Price = 10,
                StockQuantity = 5,
                CategoryId = 0, // no category
                Status = (int)ProductStatus.Available
            };

            Product addedProduct = new Product
            {
                Id = 2,
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                StockQuantity = dto.StockQuantity,
                Status = ProductStatus.Available,
                CategoryId = dto.CategoryId,
                Category = null // explicitly null
            };

            _productRepoMock.Setup(x => x.AddAsync(It.IsAny<Product>()))
                            .Callback<Product>(p => addedProduct = p)
                            .Returns(Task.CompletedTask);

            _productRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                            .ReturnsAsync(addedProduct);

            var result = await _service.CreateAsync(dto);

            result.Name.Should().Be(dto.Name);
            result.CategoryName.Should().Be(""); // ensure null category handled
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsProduct_WhenExists()
        {
            var product = new Product
            {
                Id = 1,
                Name = "Laptop",
                Description = "Gaming laptop",
                Price = 100,
                StockQuantity = 5,
                Status = ProductStatus.Available,
                CategoryId = 1,
                Category = new Category { Id = 1, Name = "Electronics" }
            };

            _productRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(product);

            var result = await _service.GetByIdAsync(1);

            result.Should().NotBeNull();
            result!.Name.Should().Be("Laptop");
            result.CategoryName.Should().Be("Electronics");
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
        {
            _productRepoMock.Setup(x => x.GetByIdAsync(2)).ReturnsAsync((Product?)null);

            var result = await _service.GetByIdAsync(2);

            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateAsync_UpdatesProduct_WhenExists()
        {
            var product = new Product
            {
                Id = 1,
                Name = "Old",
                Description = "Old Desc",
                Price = 10,
                StockQuantity = 5,
                Status = ProductStatus.Available,
                CategoryId = 1,
                Category = new Category { Id = 1, Name = "Electronics" }
            };

            var updateDto = new UpdateProductDto
            {
                Name = "Updated",
                Description = "Updated Desc",
                Price = 20,
                StockQuantity = 10,
                Status = (int)ProductStatus.Available,
                CategoryId = 1
            };

            _productRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(product);
            _productRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);

            var result = await _service.UpdateAsync(1, updateDto);

            result.Should().NotBeNull();
            result!.Name.Should().Be("Updated");
            result.Description.Should().Be("Updated Desc");
            result.Price.Should().Be(20);
        }

        [Fact]
        public async Task UpdateAsync_ReturnsNull_WhenNotExists()
        {
            _productRepoMock.Setup(x => x.GetByIdAsync(2)).ReturnsAsync((Product?)null);

            var updateDto = new UpdateProductDto
            {
                Name = "Updated",
                Description = "Updated Desc",
                Price = 20,
                StockQuantity = 10,
                Status = (int)ProductStatus.Available,
                CategoryId = 1
            };

            var result = await _service.UpdateAsync(2, updateDto);

            result.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_ReturnsFalse_WhenNotExists()
        {
            _productRepoMock.Setup(x => x.GetByIdAsync(99)).ReturnsAsync((Product?)null);

            var result = await _service.DeleteAsync(99);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteAsync_ReturnsTrue_WhenExists()
        {
            var product = new Product { Id = 1, Name = "Laptop" };
            _productRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(product);
            _productRepoMock.Setup(x => x.DeleteAsync(1)).Returns(Task.CompletedTask);

            var result = await _service.DeleteAsync(1);

            result.Should().BeTrue();
        }
    }
}
