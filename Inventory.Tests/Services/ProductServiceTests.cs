// File: Inventory.Tests\Services\ProductServiceTests.cs

using AutoMapper;
using Inventory.Application.DTOs.Product;
using Inventory.Application.Interfaces;
using Inventory.Application.Services;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Inventory.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Inventory.Tests.Services
{
    public class ProductServiceTests
    {
        private readonly Mock<IMapper> _mapperMock;
        private readonly IMapper _mapper;
        private readonly Mock<IProductRepository> _productRepoMock;
        private readonly Mock<ICacheService> _cacheMock;
        private readonly Mock<ILogger<ProductService>> _loggerMock;
        private readonly ProductService _service;

        public ProductServiceTests()
        {
            _mapperMock = new Mock<IMapper>();
            _mapper = _mapperMock.Object;
            _productRepoMock = new Mock<IProductRepository>();
            _cacheMock = new Mock<ICacheService>();
            _loggerMock = new Mock<ILogger<ProductService>>();

            _service = new ProductService(
                _productRepoMock.Object,
                _mapper,
                _cacheMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllProducts()
        {
            // Arrange
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
            _mapperMock.Setup(m => m.Map<List<ProductDto>>(It.IsAny<IEnumerable<Product>>()))
                       .Returns((IEnumerable<Product> src) => src.Select(p => new ProductDto
                       {
                           Name = p.Name,
                           CategoryName = p.Category?.Name ?? "Unknown"
                       }).ToList());

            // Act
            var result = await _service.GetAllAsync()!;

            // Assert
            var listResult = result.ToList();
            listResult.Should().HaveCount(1);
            listResult.First().Name.Should().Be("Test");
            listResult.First().CategoryName.Should().Be("Electronics");
        }

        [Fact]
        public async Task CreateAsync_AddsProduct()
        {
            // Arrange
            var dto = new CreateProductDto
            {
                Name = "New",
                Description = "Test product",
                Price = 5,
                StockQuantity = 10,
                CategoryId = 1,
                Status = (int)ProductStatus.Available
            };

            var product = new Product { Id = 1, Name = dto.Name, CategoryId = dto.CategoryId };

            _mapperMock.Setup(m => m.Map<Product>(It.IsAny<CreateProductDto>())).Returns(product);
            _productRepoMock.Setup(x => x.AddAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);
            _productRepoMock.Setup(x => x.GetByIdAsync(product.Id)).ReturnsAsync(product);
            _mapperMock.Setup(m => m.Map<ProductDto>(It.IsAny<Product>()))
                       .Returns((Product p) => new ProductDto
                       {
                           Name = p.Name,
                           CategoryName = p.Category?.Name ?? "Unknown"
                       });

            // Act
            var result = await _service.CreateAsync(dto)!;

            // Assert
            result.Name.Should().Be(dto.Name);
            result.CategoryName.Should().Be("Unknown"); // Category is null in product object
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
        {
            // Arrange
            _productRepoMock.Setup(x => x.GetByIdAsync(99)).ReturnsAsync((Product?)null);

            // Act
            var result = await _service.GetByIdAsync(99);

            // Assert
            result.Should().BeNull();
        }
    }
}
