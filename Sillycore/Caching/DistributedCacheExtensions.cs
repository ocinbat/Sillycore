using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace Sillycore.Caching
{
    public static class DistributedCacheExtensions
    {
        public static T Get<T>(this IDistributedCache cache, string key)
        {
            string entry = cache.GetString(key);

            if (String.IsNullOrWhiteSpace(entry))
            {
                return default(T);
            }

            return JsonConvert.DeserializeObject<T>(entry, SillycoreApp.JsonSerializerSettings);
        }

        public static async Task<T> GetAsync<T>(this IDistributedCache cache, string key)
        {
            string entry = await cache.GetStringAsync(key);

            if (String.IsNullOrWhiteSpace(entry))
            {
                return default(T);
            }

            return JsonConvert.DeserializeObject<T>(entry, SillycoreApp.JsonSerializerSettings);
        }

        public static void Set(this IDistributedCache cache, string key, object value)
        {
            string entry = JsonConvert.SerializeObject(value);
            cache.SetString(key, entry);
        }

        public static async Task SetAsync(this IDistributedCache cache, string key, object value)
        {
            string entry = JsonConvert.SerializeObject(value);
            await cache.SetStringAsync(key, entry);
        }

        public static void Set(this IDistributedCache cache, string key, object value, TimeSpan expiry)
        {
            string entry = JsonConvert.SerializeObject(value);
            cache.SetString(key, entry, new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = expiry
            });
        }

        public static async Task SetAsync(this IDistributedCache cache, string key, object value, TimeSpan expiry)
        {
            string entry = JsonConvert.SerializeObject(value);
            await cache.SetStringAsync(key, entry, new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = expiry
            });
        }

        public static void Set(this IDistributedCache cache, string key, object value, DistributedCacheEntryOptions options)
        {
            string entry = JsonConvert.SerializeObject(value);
            cache.SetString(key, entry, options);
        }

        public static async Task SetAsync(this IDistributedCache cache, string key, object value, DistributedCacheEntryOptions options)
        {
            string entry = JsonConvert.SerializeObject(value);
            await cache.SetStringAsync(key, entry, options);
        }
    }
}