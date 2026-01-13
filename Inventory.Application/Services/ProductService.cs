using AutoMapper;
using Inventory.Application.DTOs.Product;
using Inventory.Application.Interfaces;
using Inventory.Domain.Entities;
using Inventory.Domain.Interfaces;
using Inventory.Domain.Enums;

namespace Inventory.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;

        private const string ProductCacheKey = "products";

        public ProductService(
            IProductRepository productRepository,
            IMapper mapper,
            ICacheService cacheService)
        {
            _productRepository = productRepository;
            _mapper = mapper;
            _cacheService = cacheService;
        }

        // ============================
        // Get all products (cached)
        // ============================
        public async Task<IReadOnlyList<ProductDto>> GetAllAsync()
        {
            // 1️⃣ Try cache first
            var cached = await _cacheService.GetAsync<IReadOnlyList<ProductDto>>(ProductCacheKey);
            if (cached != null)
                return cached;

            // 2️⃣ Cache miss → load from DB
            var products = await _productRepository.GetAllAsync();

            var productDtos = products.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                Status = (int)p.Status,
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.Name ?? string.Empty
            }).ToList();

            // 3️⃣ Store in cache for future requests
            await _cacheService.SetAsync(ProductCacheKey, productDtos, TimeSpan.FromMinutes(30));

            return productDtos;
        }

        // ============================
        // Get product by Id
        // ============================
        public async Task<ProductDto?> GetByIdAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return null;

            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                Status = (int)product.Status,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name ?? string.Empty
            };
        }

        // ============================
        // Create product + invalidate cache
        // ============================
        public async Task<ProductDto> CreateAsync(CreateProductDto dto)
        {
            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description ?? string.Empty,
                Price = dto.Price,
                StockQuantity = dto.StockQuantity,
                Status = (ProductStatus)dto.Status,
                CategoryId = dto.CategoryId
            };

            await _productRepository.AddAsync(product);

            // Invalidate product list cache
            await _cacheService.RemoveAsync(ProductCacheKey);

            return await GetByIdAsync(product.Id) ??
                   _mapper.Map<ProductDto>(product);
        }

        // ============================
        // Update product + invalidate cache
        // ============================
        public async Task<ProductDto?> UpdateAsync(int id, UpdateProductDto dto)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return null;

            product.Name = dto.Name;
            product.Description = dto.Description ?? string.Empty;
            product.Price = dto.Price;
            product.StockQuantity = dto.StockQuantity;
            product.Status = (ProductStatus)dto.Status;
            product.CategoryId = dto.CategoryId;

            await _productRepository.UpdateAsync(product);

            // Invalidate product list cache
            await _cacheService.RemoveAsync(ProductCacheKey);

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

            // Invalidate product list cache
            await _cacheService.RemoveAsync(ProductCacheKey);

            return true;
        }
    }
}
