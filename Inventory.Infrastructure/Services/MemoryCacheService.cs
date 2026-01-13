using Inventory.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Inventory.Infrastructure.Services
{
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _cache;

        public MemoryCacheService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            _cache.Set(key, value, expiration ?? TimeSpan.FromMinutes(30));
            return Task.CompletedTask;
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            _cache.TryGetValue<T?>(key, out var value);
            return await Task.FromResult(value);
        }

        public async Task RemoveAsync(string key)
        {
            _cache.Remove(key);
            await Task.CompletedTask;
        }

        // =============================
        // Remove all keys with a prefix
        // =============================
        public async Task RemoveByPrefixAsync(string prefix)
        {
            var cacheItems = typeof(MemoryCache)
                .GetProperty("EntriesCollection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
                .GetValue(_cache) as dynamic;

            if (cacheItems != null)
            {
                var keysToRemove = new List<object>();
                foreach (var item in cacheItems)
                {
                    object key = item.GetType().GetProperty("Key")!.GetValue(item, null)!;
                    if (key.ToString()!.StartsWith(prefix))
                    {
                        keysToRemove.Add(key);
                    }
                }

                foreach (var key in keysToRemove)
                {
                    _cache.Remove(key);
                }
            }

            await Task.CompletedTask;
        }

    }
}
