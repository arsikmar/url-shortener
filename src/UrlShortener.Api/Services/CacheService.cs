using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using UrlShortenerApi.Models;

namespace UrlShortenerApi.Services
{
    public class CacheService
    {
        private readonly IDistributedCache _cache;

        public CacheService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<ShortenedUrl?> GetCachedUrlAsync(string code)
        {
            var cachedUrl = await _cache.GetStringAsync(code);
            if (cachedUrl == null) return null;

            var deserializedUrl = JsonSerializer.Deserialize<ShortenedUrl>(cachedUrl)!;
            return deserializedUrl;
        }

        public async Task SetCacheAsync(ShortenedUrl shortenedUrl)
        {
            var serialized = JsonSerializer.Serialize(shortenedUrl);
            await _cache.SetStringAsync(shortenedUrl.Code, serialized, new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
            });
        }
    }
}