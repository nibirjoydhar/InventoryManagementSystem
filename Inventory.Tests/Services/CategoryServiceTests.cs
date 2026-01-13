using AutoMapper;
using Inventory.Application.DTOs.Category;
using Inventory.Application.Interfaces;
using Inventory.Application.Services;
using Inventory.Domain.Entities;
using Inventory.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Inventory.Tests.Services
{
    public class CategoryServiceTests
    {
        private readonly Mock<ICategoryRepository> _categoryRepoMock;
        private readonly Mock<ICacheService> _cacheMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<CategoryService>> _loggerMock;
        private readonly CategoryService _service;

        public CategoryServiceTests()
        {
            _categoryRepoMock = new Mock<ICategoryRepository>();
            _cacheMock = new Mock<ICacheService>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<CategoryService>>();

            _service = new CategoryService(
                _categoryRepoMock.Object,
                _cacheMock.Object,
                _mapperMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllCategories()
        {
            var categories = new List<Category>
            {
                new Category { Id = 1, Name = "Electronics" },
                new Category { Id = 2, Name = "Books" }
            };

            _categoryRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(categories);
            _mapperMock.Setup(m => m.Map<IEnumerable<CategoryDto>>(It.IsAny<IEnumerable<Category>>()))
                       .Returns((IEnumerable<Category> src) => src.Select(c => new CategoryDto { Id = c.Id, Name = c.Name }).ToList());

            var result = await _service.GetAllAsync();

            result.Should().HaveCount(2);
            result.First().Name.Should().Be("Electronics");
        }

        [Fact]
        public async Task CreateAsync_AddsCategory()
        {
            var dto = new CreateCategoryDto { Name = "Toys" };
            var category = new Category { Id = 1, Name = dto.Name };

            _mapperMock.Setup(m => m.Map<Category>(dto)).Returns(category);
            _categoryRepoMock.Setup(x => x.AddAsync(It.IsAny<Category>())).Returns(Task.CompletedTask);
            _cacheMock.Setup(x => x.RemoveAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
            _mapperMock.Setup(m => m.Map<CategoryDto>(category)).Returns(new CategoryDto { Id = category.Id, Name = category.Name });

            var result = await _service.CreateAsync(dto);

            result.Name.Should().Be(dto.Name);
        }

        [Fact]
        public async Task CreateAsync_ThrowsException_WhenNameIsEmpty()
        {
            var dto = new CreateCategoryDto { Name = "" };

            Func<Task> act = async () => await _service.CreateAsync(dto);

            await act.Should().ThrowAsync<ArgumentException>();
            _categoryRepoMock.Verify(x => x.AddAsync(It.IsAny<Category>()), Times.Never);
        }
    }
}
