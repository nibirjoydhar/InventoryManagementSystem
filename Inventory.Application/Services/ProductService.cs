using AutoMapper;
using Inventory.Application.DTOs.Product;
using Inventory.Application.Interfaces;
using Inventory.Domain.Entities;
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

        public async Task<(int Total, IReadOnlyList<ProductDto> Items)> GetAllAsync(ProductQueryParamsDto? queryParams = null)
        {
            queryParams ??= new ProductQueryParamsDto();

            var cacheKey = $"{ProductCacheKeyPrefix}_p{queryParams.Page}_s{queryParams.PageSize}" +
                           $"_min{queryParams.MinPrice}_max{queryParams.MaxPrice}" +
                           $"_cat{queryParams.CategoryId}_sort{queryParams.SortBy}_asc{queryParams.Ascending}";

            var cached = await _cacheService.GetAsync<(int Total, IReadOnlyList<ProductDto> Items)>(cacheKey);
            if (cached.Items?.Count > 0)
            {
                _logger.LogInformation("Products retrieved from cache: {CacheKey}", cacheKey);
                return cached;
            }

            var (total, products) = await _productRepository.GetAllWithTotalAsync(queryParams);

            var productDtos = _mapper.Map<List<ProductDto>>(products);

            var result = (total, productDtos.AsReadOnly());

            await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(30));
            _logger.LogInformation("Products fetched from DB and cached: {CacheKey}", cacheKey);

            return result;
        }

        public async Task<ProductDto?> GetByIdAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return null;
            return _mapper.Map<ProductDto>(product);
        }

        public async Task<ProductDto> CreateAsync(CreateProductDto dto)
        {
            var product = _mapper.Map<Product>(dto);
            await _productRepository.AddAsync(product);

            await _cacheService.RemoveByPrefixAsync(ProductCacheKeyPrefix);
            _logger.LogInformation("Product created: ID {ProductId}", product.Id);

            return await GetByIdAsync(product.Id) ?? _mapper.Map<ProductDto>(product);
        }

        public async Task<ProductDto?> UpdateAsync(int id, UpdateProductDto dto)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return null;

            _mapper.Map(dto, product);
            await _productRepository.UpdateAsync(product);

            await _cacheService.RemoveByPrefixAsync(ProductCacheKeyPrefix);
            _logger.LogInformation("Product updated: ID {ProductId}", product.Id);

            return await GetByIdAsync(product.Id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return false;

            await _productRepository.DeleteAsync(id);

            await _cacheService.RemoveByPrefixAsync(ProductCacheKeyPrefix);
            _logger.LogInformation("Product deleted: ID {ProductId}", id);

            return true;
        }
    }
}