using Inventory.Application.DTOs.Category;
using Inventory.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Inventory.Api.Controllers
{
    /// <summary>
    /// Controller for managing categories
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoryController> _logger;
        private readonly ICacheService _cacheService;
        private const string CategoryListCacheKey = "category_list";

        /// <summary>
        /// Constructor for CategoryController
        /// </summary>
        /// <param name="categoryService">Category service</param>
        /// <param name="logger">Logger instance</param>
        /// <param name="cacheService">Cache service</param>
        public CategoryController(
            ICategoryService categoryService,
            ILogger<CategoryController> logger,
            ICacheService cacheService)
        {
            _categoryService = categoryService;
            _logger = logger;
            _cacheService = cacheService;
        }

        /// <summary>
        /// Get all categories
        /// </summary>
        /// <returns>List of categories with total count</returns>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var cached = await _cacheService.GetAsync<IEnumerable<CategoryDto>>(CategoryListCacheKey);
            if (cached != null)
                return Ok(cached);

            var categories = await _categoryService.GetAllAsync();

            await _cacheService.SetAsync(CategoryListCacheKey, categories, TimeSpan.FromMinutes(10));

            return Ok(new
            {
                Total = categories.Count,
                Data = categories
            });
        }

        /// <summary>
        /// Get category by ID
        /// </summary>
        /// <param name="id">Category ID</param>
        /// <returns>Category details if found; NotFound otherwise</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category == null) return NotFound();
            return Ok(category);
        }

        /// <summary>
        /// Create a new category (Admin only)
        /// </summary>
        /// <param name="dto">Category creation data</param>
        /// <returns>Created category</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var created = await _categoryService.CreateAsync(dto);

            _logger.LogInformation("Category created: {CategoryName} (ID: {CategoryId}) at {Time}",
                created.Name, created.Id, DateTime.UtcNow);

            // Invalidate cache
            await _cacheService.RemoveAsync(CategoryListCacheKey);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
    }
}
