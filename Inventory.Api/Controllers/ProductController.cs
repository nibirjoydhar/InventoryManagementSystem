using Inventory.Application.DTOs.Product;
using Inventory.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Inventory.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductController> _logger;
        private readonly ICacheService _cacheService;

        // Use same prefix as service for proper invalidation
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

        [HttpGet]
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
                // You can add: SortBy = "name", Ascending = true as defaults if needed
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null) return NotFound();
            return Ok(product);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var created = await _productService.CreateAsync(dto);

            _logger.LogInformation("Product created: {Name} (ID: {Id})", created.Name, created.Id);

            await _cacheService.RemoveByPrefixAsync(ProductCacheKeyPrefix);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var updated = await _productService.UpdateAsync(id, dto);
            if (updated == null) return NotFound();

            _logger.LogInformation("Product updated: {Name} (ID: {Id})", updated.Name, updated.Id);

            await _cacheService.RemoveByPrefixAsync(ProductCacheKeyPrefix);

            return Ok(updated);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
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