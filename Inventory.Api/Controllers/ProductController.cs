using Inventory.Application.DTOs.Product;
using Inventory.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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
        private const string ProductListCacheKey = "product_list";

        /// <summary>
        /// Constructor for ProductController
        /// </summary>
        /// <param name="productService">Product service</param>
        /// <param name="logger">Logger instance</param>
        /// <param name="cacheService">Cache service</param>
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
        /// <param name="page">Page number (default 1)</param>
        /// <param name="pageSize">Page size (default 10)</param>
        /// <param name="minPrice">Minimum price filter</param>
        /// <param name="maxPrice">Maximum price filter</param>
        /// <param name="categoryId">Category ID filter</param>
        /// <returns>Paged list of products</returns>
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] double? minPrice = null,
            [FromQuery] double? maxPrice = null,
            [FromQuery] int? categoryId = null)
        {
            var cacheKey = $"{ProductListCacheKey}_p{page}_s{pageSize}_min{minPrice}_max{maxPrice}_c{categoryId}";
            var cached = await _cacheService.GetAsync<IEnumerable<ProductDto>>(cacheKey);
            if (cached != null) return Ok(cached);

            var products = await _productService.GetAllAsync();

            if (minPrice.HasValue)
                products = products.Where(p => p.Price >= (decimal)minPrice.Value).ToList();
            if (maxPrice.HasValue)
                products = products.Where(p => p.Price <= (decimal)maxPrice.Value).ToList();
            if (categoryId.HasValue)
                products = products.Where(p => p.CategoryId == categoryId.Value).ToList();

            var total = products.Count;
            var paged = products.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            await _cacheService.SetAsync(cacheKey, paged, TimeSpan.FromMinutes(5));

            return Ok(new
            {
                Total = total,
                Page = page,
                PageSize = pageSize,
                Data = paged
            });
        }

        /// <summary>
        /// Get product by ID
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <returns>Product details if found; NotFound otherwise</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null) return NotFound();
            return Ok(product);
        }

        /// <summary>
        /// Create a new product (Admin only)
        /// </summary>
        /// <param name="dto">Product creation data</param>
        /// <returns>Created product</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var created = await _productService.CreateAsync(dto);

            _logger.LogInformation("Product created: {ProductName} (ID: {ProductId}) at {Time}",
                created.Name, created.Id, DateTime.UtcNow);

            await _cacheService.RemoveAsync(ProductListCacheKey);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Update an existing product (Admin only)
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <param name="dto">Update data</param>
        /// <returns>Updated product if successful; NotFound otherwise</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var updated = await _productService.UpdateAsync(id, dto);
            if (updated == null) return NotFound();

            _logger.LogInformation("Product updated: {ProductName} (ID: {ProductId}) at {Time}",
                updated.Name, updated.Id, DateTime.UtcNow);

            await _cacheService.RemoveAsync(ProductListCacheKey);

            return Ok(updated);
        }

        /// <summary>
        /// Delete a product (Admin only)
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <returns>No content if deleted; NotFound otherwise</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _productService.DeleteAsync(id);
            if (!result) return NotFound();

            _logger.LogInformation("Product deleted: ID {ProductId} at {Time}", id, DateTime.UtcNow);

            await _cacheService.RemoveAsync(ProductListCacheKey);

            return NoContent();
        }
    }
}
