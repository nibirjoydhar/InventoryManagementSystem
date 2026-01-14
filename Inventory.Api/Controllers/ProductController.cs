using Inventory.Application.DTOs.Product;
using Inventory.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;

namespace Inventory.Api.Controllers
{
    /// <summary>
    /// Controller for managing products
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductController> _logger;
        private readonly ICacheService _cacheService;

        private const string ProductCacheKeyPrefix = "products";

        public ProductController(
            IProductService productService,
            ILogger<ProductController> logger,
            ICacheService cacheService)
        {
            _productService = productService;
            _logger = logger;
            _cacheService = cacheService;
        }

        /// <summary>
        /// Get all products with optional filters and pagination
        /// </summary>
        [HttpGet]
        [SwaggerOperation(
            Summary = "Get all products",
            Description = "Returns paginated products with optional price and category filters"
        )]
        [ProducesResponseType(200)]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] double? minPrice = null,
            [FromQuery] double? maxPrice = null,
            [FromQuery] int? categoryId = null)
        {
            var queryParams = new ProductQueryParamsDto
            {
                Page = page,
                PageSize = pageSize,
                MinPrice = minPrice.HasValue ? (decimal?)minPrice.Value : null,
                MaxPrice = maxPrice.HasValue ? (decimal?)maxPrice.Value : null,
                CategoryId = categoryId
            };

            var (total, data) = await _productService.GetAllAsync(queryParams);

            return Ok(new
            {
                Total = total,
                Page = page,
                PageSize = pageSize,
                Data = data
            });
        }

        /// <summary>
        /// Get product by ID
        /// </summary>
        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Get product by ID")]
        [ProducesResponseType(typeof(ProductDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null) return NotFound();
            return Ok(product);
        }

        /// <summary>
        /// Create a new product (Admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(Summary = "Create a product")]
        [ProducesResponseType(typeof(ProductDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var created = await _productService.CreateAsync(dto);

            _logger.LogInformation("Product created: {Name} (ID: {Id})", created.Name, created.Id);

            await _cacheService.RemoveByPrefixAsync(ProductCacheKeyPrefix);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Update a product (Admin only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(Summary = "Update a product")]
        [ProducesResponseType(typeof(ProductDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var updated = await _productService.UpdateAsync(id, dto);
            if (updated == null) return NotFound();

            _logger.LogInformation("Product updated: {Name} (ID: {Id})", updated.Name, updated.Id);

            await _cacheService.RemoveByPrefixAsync(ProductCacheKeyPrefix);

            return Ok(updated);
        }

        /// <summary>
        /// Delete a product (Admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(Summary = "Delete a product")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _productService.DeleteAsync(id);
            if (!result) return NotFound();

            _logger.LogInformation("Product deleted: ID {Id}", id);

            await _cacheService.RemoveByPrefixAsync(ProductCacheKeyPrefix);

            return NoContent();
        }
    }
}
