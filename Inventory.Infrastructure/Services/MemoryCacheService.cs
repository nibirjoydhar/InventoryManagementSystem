using Inventory.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Inventory.Infrastructure.Services
{
    /// <summary>
    /// In-memory cache service with support for prefix-based removal
    /// </summary>
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _cache;

        // Track keys internally for safe RemoveByPrefix
        private readonly HashSet<string> _keys = new();

        public MemoryCacheService(IMemoryCache cache)
        {
            _cache = cache;
        }

        /// <summary>
        /// Set a cache entry
        /// </summary>
        /// <typeparam name="T">Type of value</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="value">Value to cache</param>
        /// <param name="expiration">Optional expiration time</param>
        public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            _cache.Set(key, value, expiration ?? TimeSpan.FromMinutes(30));
            _keys.Add(key); // Track key for prefix removal
            return Task.CompletedTask;
        }

        /// <summary>
        /// Get a cached entry
        /// </summary>
        /// <typeparam name="T">Type of value</typeparam>
        /// <param name="key">Cache key</param>
        /// <returns>Cached value or null if not found</returns>
        public Task<T?> GetAsync<T>(string key)
        {
            _cache.TryGetValue<T?>(key, out var value);
            return Task.FromResult(value);
        }

        /// <summary>
        /// Remove a specific cache entry
        /// </summary>
        /// <param name="key">Cache key</param>
        public Task RemoveAsync(string key)
        {
            _cache.Remove(key);
            _keys.Remove(key); // Remove from tracking
            return Task.CompletedTask;
        }

        /// <summary>
        /// Remove all cache entries starting with a prefix
        /// </summary>
        /// <param name="prefix">Prefix string</param>
        public Task RemoveByPrefixAsync(string prefix)
        {
            var keysToRemove = _keys.Where(k => k.StartsWith(prefix)).ToList();
            foreach (var key in keysToRemove)
            {
                _cache.Remove(key);
                _keys.Remove(key);
            }

            return Task.CompletedTask;
        }
    }
}
