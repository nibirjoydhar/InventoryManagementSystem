using Inventory.Application.DTOs.Category;
using Inventory.Application.Interfaces;
using Inventory.Domain.Entities;
using Inventory.Domain.Interfaces;

namespace Inventory.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        // Get all categories
        public async Task<IReadOnlyList<CategoryDto>> GetAllAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return categories.Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name
            }).ToList();
        }

        // Get category by Id
        public async Task<CategoryDto?> GetByIdAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null) return null;

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name
            };
        }

        // Create new category
        public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto)
        {
            // 🚨 Validation: Name is required
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                throw new ArgumentException("Category name is required");
            }

            // Create category entity
            var category = new Category
            {
                Name = dto.Name.Trim() // Remove accidental whitespace
            };

            // Save to repository
            await _categoryRepository.AddAsync(category);

            // Return DTO
            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name
            };
        }
    }
}
