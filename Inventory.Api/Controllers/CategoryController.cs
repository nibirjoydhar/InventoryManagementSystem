    using Inventory.Application.DTOs.Category;
    using Inventory.Application.Interfaces;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Swashbuckle.AspNetCore.Annotations;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System;

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
            /// <response code="200">Returns list of categories</response>
            [HttpGet]
            [SwaggerOperation(Summary = "Get all categories", Description = "Returns a paginated list of all categories with total count")]
            [ProducesResponseType(typeof(IEnumerable<CategoryDto>), 200)]
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
            /// <response code="200">Returns the category details</response>
            /// <response code="404">Category not found</response>
            [HttpGet("{id}")]
            [SwaggerOperation(Summary = "Get a category by ID", Description = "Returns a single category details if found")]
            [ProducesResponseType(typeof(CategoryDto), 200)]
            [ProducesResponseType(404)]
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
            /// <response code="201">Category created successfully</response>
            /// <response code="400">Invalid request data</response>
            /// <response code="401">Unauthorized if user is not Admin</response>
            [HttpPost]
            [Authorize(Roles = "Admin")]
            [SwaggerOperation(Summary = "Create a new category", Description = "Creates a new category. Admin role required.")]
            [ProducesResponseType(typeof(CategoryDto), 201)]
            [ProducesResponseType(typeof(ErrorResponseDto), 400)]
            [ProducesResponseType(typeof(ErrorResponseDto), 401)]
            public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
            {
                if (!ModelState.IsValid) return BadRequest(new ErrorResponseDto { Message = "Invalid request data" });

                var created = await _categoryService.CreateAsync(dto);

                _logger.LogInformation("Category created: {CategoryName} (ID: {CategoryId}) at {Time}",
                    created.Name, created.Id, DateTime.UtcNow);

                // Invalidate cache
                await _cacheService.RemoveAsync(CategoryListCacheKey);

                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
        }

        /// <summary>
        /// Standard error response for API
        /// </summary>
        public class ErrorResponseDto
        {
            /// <summary>
            /// Error message
            /// </summary>
            public string Message { get; set; } = null!;
        }
    }
