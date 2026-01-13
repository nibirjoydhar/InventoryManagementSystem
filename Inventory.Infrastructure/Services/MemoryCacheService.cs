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

        //public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        //{
        //    _cache.Set(key, value, expiration ?? TimeSpan.FromMinutes(30));
        //    await Task.CompletedTask;
        //}
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
    }
}
