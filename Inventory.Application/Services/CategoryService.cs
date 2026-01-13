using AutoMapper;
using Inventory.Application.DTOs.Category;
using Inventory.Application.Interfaces;
using Inventory.Domain.Entities;
using Inventory.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Inventory.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ICacheService _cacheService;
        private readonly IMapper _mapper;
        private readonly ILogger<CategoryService> _logger;

        private const string CategoryCacheKey = "categories";

        public CategoryService(
            ICategoryRepository categoryRepository,
            ICacheService cacheService,
            IMapper mapper,
            ILogger<CategoryService> logger)
        {
            _categoryRepository = categoryRepository;
            _cacheService = cacheService;
            _mapper = mapper;
            _logger = logger;
        }

        // ================================
        // Get all categories (cached)
        // ================================
        public async Task<IReadOnlyList<CategoryDto>> GetAllAsync()
        {
            var cached = await _cacheService.GetAsync<IReadOnlyList<CategoryDto>>(CategoryCacheKey);
            if (cached != null)
            {
                _logger.LogInformation("Categories retrieved from cache");
                return cached;
            }

            var categories = await _categoryRepository.GetAllAsync();
            var dtos = _mapper.Map<List<CategoryDto>>(categories);

            await _cacheService.SetAsync(CategoryCacheKey, dtos, TimeSpan.FromMinutes(30));

            _logger.LogInformation("Categories retrieved from DB and cached");

            return dtos;
        }

        // ================================
        // Get category by Id
        // ================================
        public async Task<CategoryDto?> GetByIdAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null) return null;

            return _mapper.Map<CategoryDto>(category);
        }

        // ================================
        // Create new category (Admin)
        // ================================
        public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                throw new ArgumentException("Category name is required");
            }

            var category = _mapper.Map<Category>(dto);
            await _categoryRepository.AddAsync(category);

            // Invalidate cache
            await _cacheService.RemoveAsync(CategoryCacheKey);

            _logger.LogInformation("Category created: {CategoryId}", category.Id);

            return _mapper.Map<CategoryDto>(category);
        }
    }
}
