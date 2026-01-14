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
            var queryParams = new ProductQueryParamsDto { Page = 1, PageSize = 10 };

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

            var expectedTotal = 1; // Total matching records
            var expectedItems = products.AsReadOnly();

            // Mock repository to return tuple
            _productRepoMock.Setup(x => x.GetAllWithTotalAsync(It.IsAny<ProductQueryParamsDto>()))
                            .ReturnsAsync((expectedTotal, expectedItems));

            // Mock mapper
            _mapperMock.Setup(m => m.Map<List<ProductDto>>(It.IsAny<IEnumerable<Product>>()))
                       .Returns((IEnumerable<Product> src) => src.Select(p => new ProductDto
                       {
                           Name = p.Name,
                           CategoryName = p.Category?.Name ?? "Unknown"
                       }).ToList());

            // Act
            var (total, items) = await _service.GetAllAsync(queryParams);

            // Assert
            total.Should().Be(expectedTotal);
            items.Should().HaveCount(1);
            items.First().Name.Should().Be("Test");
            items.First().CategoryName.Should().Be("Electronics");
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

            var product = new Product
            {
                Id = 1,
                Name = dto.Name,
                CategoryId = dto.CategoryId,
                Category = new Category { Name = "Electronics" } // Simulate loaded category
            };

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
            var result = await _service.CreateAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(dto.Name);
            result.CategoryName.Should().Be("Electronics"); // Now reflects loaded category
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

        // Optional: Add a test for cache hit scenario if you want full coverage
        [Fact]
        public async Task GetAllAsync_ReturnsFromCache_WhenAvailable()
        {
            // Arrange
            var queryParams = new ProductQueryParamsDto { Page = 1, PageSize = 10 };
            var cachedItems = new List<ProductDto> { new ProductDto { Name = "Cached" } }.AsReadOnly();
            var cachedTuple = (Total: 5, Items: cachedItems);

            _cacheMock.Setup(c => c.GetAsync<(int Total, IReadOnlyList<ProductDto> Items)>(It.IsAny<string>()))
                      .ReturnsAsync(cachedTuple);

            // Act
            var (total, items) = await _service.GetAllAsync(queryParams);

            // Assert
            total.Should().Be(5);
            items.Should().HaveCount(1);
            items.First().Name.Should().Be("Cached");
            _productRepoMock.Verify(r => r.GetAllWithTotalAsync(It.IsAny<ProductQueryParamsDto>()), Times.Never);
        }
    }
}