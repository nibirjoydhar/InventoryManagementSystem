using Inventory.Application.DTOs.Category;
using Inventory.Application.Services;
using Inventory.Domain.Entities;
using Inventory.Domain.Interfaces;
using Moq;
using Xunit;
using FluentAssertions;

namespace Inventory.Tests.Services
{
    public class CategoryServiceTests
    {
        private readonly Mock<ICategoryRepository> _categoryRepoMock;
        private readonly CategoryService _service;

        public CategoryServiceTests()
        {
            _categoryRepoMock = new Mock<ICategoryRepository>();
            _service = new CategoryService(_categoryRepoMock.Object);
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

            var result = await _service.GetAllAsync();

            result.Should().HaveCount(2);
            result[0].Name.Should().Be("Electronics");
        }

        [Fact]
        public async Task CreateAsync_AddsCategory()
        {
            var dto = new CreateCategoryDto { Name = "Toys" };
            Category addedCategory = null!;
            _categoryRepoMock.Setup(x => x.AddAsync(It.IsAny<Category>()))
                             .Callback<Category>(c => addedCategory = c)
                             .Returns(Task.CompletedTask);

            var result = await _service.CreateAsync(dto);

            result.Name.Should().Be(dto.Name);
            addedCategory.Name.Should().Be(dto.Name);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsEmptyList_WhenNoCategories()
        {
            // Arrange
            _categoryRepoMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<Category>());

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }


        [Fact]
        public async Task CreateAsync_ThrowsException_WhenNameIsEmpty()
        {
            // Arrange
            var dto = new CreateCategoryDto { Name = "" };

            // Act
            Func<Task> act = async () => await _service.CreateAsync(dto);

            // Assert
            await act.Should()
                .ThrowAsync<ArgumentException>();

            _categoryRepoMock.Verify(
                x => x.AddAsync(It.IsAny<Category>()),
                Times.Never
            );
        }

    }
}
