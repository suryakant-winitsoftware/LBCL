using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using WINITServices.Interfaces.CacheHandler;

namespace WINITServices.Classes.CacheHandler
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;

        public RedisCacheService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public T Get<T>(string key)
        {
            var value = _cache.GetString(key);
            if (value != null)
            {
                return JsonSerializer.Deserialize<T>(value);
            }
            return default;
        }

        public void Set<T>(string key, T value, ExpirationType expirationType = ExpirationType.Absolute, int seconds = 600)
        {
            var options = new DistributedCacheEntryOptions();
            if (expirationType == ExpirationType.Absolute)
            {
                options.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(seconds);
            }
            else if (expirationType == ExpirationType.Sliding)
            {
                options.SlidingExpiration = TimeSpan.FromSeconds(seconds);
            }

            var serializedValue = JsonSerializer.Serialize(value);
            _cache.SetString(key, serializedValue, options);
        }

        public void Remove(string key)
        {
            _cache.Remove(key);
        }

        public void InvalidateCache(string invalidateKeyType, string cacheKey)
        {
            _cache.Remove(cacheKey);
        }
        public void InvalidateCacheByPrefix(string cacheKeyPrefix)
        {
            //var cacheKeys = GetAllCacheKeys();
            //var keysToRemove = cacheKeys.Where(key => key.StartsWith(cacheKeyPrefix)).ToList();

            //foreach (var key in keysToRemove)
            //{
            //    _cache.Remove(key);
            //}
        }

        public Dictionary<string, T> GetMultiple<T>(List<string> keys)
        {
            throw new NotImplementedException();
        }

        public void HSet<T>(string key, string field, T value, ExpirationType expirationType = ExpirationType.Absolute, int seconds = 0)
        {
            throw new NotImplementedException();
        }

        public T HGet<T>(string key, string field)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, T> HGetAll<T>(string key)
        {
            throw new NotImplementedException();
        }

        public void LSet<T>(string key, long index, T value, ExpirationType expirationType = ExpirationType.Absolute, int seconds = 0)
        {
            throw new NotImplementedException();
        }

        public T LIndex<T>(string key, long index)
        {
            throw new NotImplementedException();
        }

        public void ListRightPush<T>(string key, List<T> values, ExpirationType expirationType = ExpirationType.Absolute, int seconds = 0)
        {
            throw new NotImplementedException();
        }

        public void SAdd<T>(string key, List<T> members, ExpirationType expirationType = ExpirationType.Absolute, int seconds = 0)
        {
            throw new NotImplementedException();
        }

        public List<T> SMembers<T>(string key)
        {
            throw new NotImplementedException();
        }

        public List<T> LRange<T>(string key, long start, long stop)
        {
            throw new NotImplementedException();
        }

        public void InvalidateCache(string cacheKey)
        {
            throw new NotImplementedException();
        }

        public void SAdd<T>(string key, T member, ExpirationType expirationType = ExpirationType.Absolute, int seconds = 0)
        {
            throw new NotImplementedException();
        }

        public List<string> GetKeyByPattern(string keyPattern)
        {
            throw new NotImplementedException();
        }

        public void SetKeyExpirationTime(string key, ExpirationType expirationType = ExpirationType.Absolute, int seconds = -1)
        {
            throw new NotImplementedException();
        }

        public List<string> GetHashValueByPattern(string key, string keyPattern)
        {
            throw new NotImplementedException();
        }

        public List<T> HGet<T>(string key, List<string> fields)
        {
            throw new NotImplementedException();
        }
    }
}
