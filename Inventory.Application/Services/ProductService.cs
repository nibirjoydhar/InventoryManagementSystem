using AutoMapper;
using Inventory.Application.DTOs.Product;
using Inventory.Application.Interfaces;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Inventory.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Inventory.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;
        private readonly ILogger<ProductService> _logger;

        private const string ProductCacheKeyPrefix = "products";

        public ProductService(
            IProductRepository productRepository,
            IMapper mapper,
            ICacheService cacheService,
            ILogger<ProductService> logger)
        {
            _productRepository = productRepository;
            _mapper = mapper;
            _cacheService = cacheService;
            _logger = logger;
        }

        // ============================
        // Get all products with filtering, sorting, pagination, caching
        // ============================
        public async Task<IReadOnlyList<ProductDto>> GetAllAsync(ProductQueryParamsDto? queryParams = null)
        {
            queryParams ??= new ProductQueryParamsDto();

            var cacheKey = $"{ProductCacheKeyPrefix}_page{queryParams.Page}_size{queryParams.PageSize}" +
                           $"_min{queryParams.MinPrice}_max{queryParams.MaxPrice}" +
                           $"_cat{queryParams.CategoryId}_sort{queryParams.SortBy}_asc{queryParams.Ascending}";

            var cached = await _cacheService.GetAsync<IReadOnlyList<ProductDto>>(cacheKey);
            if (cached != null)
            {
                _logger.LogInformation("Products retrieved from cache: {CacheKey}", cacheKey);
                return cached;
            }

            var products = await _productRepository.GetAllAsync();

            // ----------------------------
            // Filtering
            // ----------------------------
            if (queryParams.MinPrice.HasValue)
                products = products.Where(p => p.Price >= queryParams.MinPrice.Value).ToList();

            if (queryParams.MaxPrice.HasValue)
                products = products.Where(p => p.Price <= queryParams.MaxPrice.Value).ToList();

            if (queryParams.CategoryId.HasValue)
                products = products.Where(p => p.CategoryId == queryParams.CategoryId.Value).ToList();

            // ----------------------------
            // Sorting
            // ----------------------------
            products = queryParams.SortBy.ToLower() switch
            {
                "name" => queryParams.Ascending ? products.OrderBy(p => p.Name).ToList() : products.OrderByDescending(p => p.Name).ToList(),
                "price" => queryParams.Ascending ? products.OrderBy(p => p.Price).ToList() : products.OrderByDescending(p => p.Price).ToList(),
                "id" => queryParams.Ascending ? products.OrderBy(p => p.Id).ToList() : products.OrderByDescending(p => p.Id).ToList(),
                _ => products
            };

            // ----------------------------
            // Pagination
            // ----------------------------
            products = products
                .Skip((queryParams.Page - 1) * queryParams.PageSize)
                .Take(queryParams.PageSize)
                .ToList();

            var productDtos = _mapper.Map<List<ProductDto>>(products);

            // ----------------------------
            // Cache the result
            // ----------------------------
            await _cacheService.SetAsync(cacheKey, productDtos, TimeSpan.FromMinutes(30));
            _logger.LogInformation("Products retrieved from DB and cached: {CacheKey}", cacheKey);

            return productDtos;
        }

        // ============================
        // Get single product by Id
        // ============================
        public async Task<ProductDto?> GetByIdAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return null;

            return _mapper.Map<ProductDto>(product);
        }

        // ============================
        // Create product + invalidate cache
        // ============================
        public async Task<ProductDto> CreateAsync(CreateProductDto dto)
        {
            var product = _mapper.Map<Product>(dto);

            await _productRepository.AddAsync(product);

            // Invalidate all product caches
            await _cacheService.RemoveByPrefixAsync(ProductCacheKeyPrefix);

            _logger.LogInformation("Product created: {ProductId}", product.Id);

            return await GetByIdAsync(product.Id) ?? _mapper.Map<ProductDto>(product);
        }

        // ============================
        // Update product + invalidate cache
        // ============================
        public async Task<ProductDto?> UpdateAsync(int id, UpdateProductDto dto)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return null;

            _mapper.Map(dto, product);
            await _productRepository.UpdateAsync(product);

            // Invalidate all product caches
            await _cacheService.RemoveByPrefixAsync(ProductCacheKeyPrefix);

            _logger.LogInformation("Product updated: {ProductId}", product.Id);

            return await GetByIdAsync(product.Id);
        }

        // ============================
        // Delete product + invalidate cache
        // ============================
        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return false;

            await _productRepository.DeleteAsync(id);

            // Invalidate all product caches
            await _cacheService.RemoveByPrefixAsync(ProductCacheKeyPrefix);

            _logger.LogInformation("Product deleted: {ProductId}", id);

            return true;
        }
    }
}
