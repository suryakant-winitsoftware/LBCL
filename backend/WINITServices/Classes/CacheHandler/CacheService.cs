using System;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using WINITServices.Interfaces.CacheHandler;

namespace WINITServices.Classes.CacheHandler
{
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _cache;

        public CacheService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public T Get<T>(string key)
        {
            return _cache.Get<T>(key);
        }
        /// <summary>
        /// Set a cache with key. Default expiration time 600 seconds and expiration type is Absolute
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expirationType"></param>
        /// <param name="seconds"></param>
        public void Set<T>(string key, T value, ExpirationType expirationType = ExpirationType.Absolute, int seconds = 600)
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions();
            if (expirationType == ExpirationType.Absolute)
            {
                cacheEntryOptions.SetAbsoluteExpiration(TimeSpan.FromSeconds(seconds));
            }
            else if (expirationType == ExpirationType.Sliding)
            {
                cacheEntryOptions.SetSlidingExpiration(TimeSpan.FromSeconds(seconds));
            }
            _cache.Set(key, value, cacheEntryOptions);
        }

        public void Remove(string key)
        {
            _cache.Remove(key);
        }
        public void InvalidateCache(string invalidateKeyType, string cacheKey)
        {
            _cache.Remove(cacheKey);
        }
        /// <summary>
        /// Method to remove all cache with key started with given cacheKeyPrefix
        /// </summary>
        /// <param name="cacheKeyPrefix"></param>
        public void InvalidateCacheByPrefix(string cacheKeyPrefix)
        {
            var keysToRemove = new List<string>();
            var field = typeof(MemoryCache).GetProperty("EntriesCollection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                var collection = field.GetValue(_cache) as dynamic;
                if (collection != null)
                {
                    foreach (var item in collection)
                    {
                        var methodInfo = item.GetType().GetProperty("Key");
                        var cacheKey = methodInfo.GetValue(item) as string;
                        if (cacheKey.StartsWith(cacheKeyPrefix))
                        {
                            keysToRemove.Add(cacheKey);
                        }
                    }
                }
            }

            foreach (var key in keysToRemove)
            {
                _cache.Remove(key);
            }
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