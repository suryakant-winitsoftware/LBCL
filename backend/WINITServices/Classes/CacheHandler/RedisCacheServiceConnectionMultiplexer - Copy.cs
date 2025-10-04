using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using WINITServices.Interfaces.CacheHandler;
using WINITSharedObjects.Constants;

namespace WINITServices.Classes.CacheHandler
{
    public class RedisCacheServiceConnectionMultiplexer : ICacheService
    {
        private readonly ConnectionMultiplexer _cache;
        private const int defaultSeconds = 600;

        public RedisCacheServiceConnectionMultiplexer(ConnectionMultiplexer cache)
        {
            _cache = cache;
        }
        /// <summary>
        /// GetTimeSpan based on key, expirationType, seconds
        /// </summary>
        /// <param name="key"></param>
        /// <param name="expirationType"></param>
        /// <param name="seconds"></param>
        /// <returns></returns>
        private TimeSpan? GetTimeSpan(string key, ExpirationType expirationType = ExpirationType.Absolute, int seconds = defaultSeconds)
        {
            var db = _cache.GetDatabase();
            TimeSpan? expiry = null;

            if (expirationType == ExpirationType.Absolute)
            {
                expiry = TimeSpan.FromSeconds(seconds);
            }
            else if (expirationType == ExpirationType.Sliding)
            {
                var existingTtl = db.KeyTimeToLive(key);
                if (existingTtl.HasValue && existingTtl.Value.TotalSeconds > seconds)
                {
                    expiry = existingTtl.Value;
                }
                else
                {
                    expiry = TimeSpan.FromSeconds(seconds);
                }
            }
            return expiry;
        }
        /// <summary>
        /// The Get method is a generic method that takes a key (string key) and specified object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Get<T>(string key)
        {
            var db = _cache.GetDatabase();
            var value = db.StringGet(key);
            if (value.HasValue)
            {
                return JsonSerializer.Deserialize<T>(value);
            }
            return default;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expirationType"></param>
        /// <param name="seconds"></param>
        public void Set<T>(string key, T value, ExpirationType expirationType = ExpirationType.Absolute, int seconds = defaultSeconds)
        {
            var db = _cache.GetDatabase();
            var serializedValue = JsonSerializer.Serialize(value);

            TimeSpan? expiry = GetTimeSpan(key, expirationType, seconds);
            db.StringSet(key, serializedValue, expiry);
        }
        /// <summary>
        /// The GetMultiple method is a generic method that takes a list of keys (List<string> keys) and returns a dictionary with the key-value pairs.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keys"></param>
        /// <returns></returns>
        public Dictionary<string, T> GetMultiple<T>(List<string> keys)
        {
            var db = _cache.GetDatabase();
            RedisKey[] redisKeys = keys.Select(k => (RedisKey)k).ToArray();
            RedisValue[] redisValues = db.StringGet(redisKeys);
            var results = new Dictionary<string, T>();

            for (int i = 0; i < redisValues.Length; i++)
            {
                var key = keys[i];
                var value = redisValues[i];

                if (value.HasValue)
                {
                    var deserializedValue = JsonSerializer.Deserialize<T>(value);
                    results.Add(key, deserializedValue);
                }
            }

            return results;
        }
        /// <summary>
        /// The HSet method is a generic method that takes a key, a field, and a value as parameters to set the value for a specific field in a hash.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="field"></param>
        /// <param name="value"></param>
        public void HSet<T>(string key, string field, T value, ExpirationType expirationType = ExpirationType.Absolute, int seconds = defaultSeconds)
        {
            var db = _cache.GetDatabase();
            var serializedValue = JsonSerializer.Serialize(value);
            db.HashSet(key, field, serializedValue);

            TimeSpan? expiry = GetTimeSpan(key, expirationType, seconds);
            db.KeyExpire(key, expiry);
        }
        /// <summary>
        /// The HGet method is a generic method that takes a key and a field within the hash as parameters and returns the value associated with that field.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public T HGet<T>(string key, string field)
        {
            var db = _cache.GetDatabase();
            var value = db.HashGet(key, field);
            if (value.HasValue)
            {
                return JsonSerializer.Deserialize<T>(value);
            }
            return default;
        }
        /// <summary>
        /// The HGetAll method is a generic method that takes a key as a parameter and returns a dictionary with the field-value pairs from the hash.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public Dictionary<string, T> HGetAll<T>(string key)
        {
            var db = _cache.GetDatabase();
            var hashEntries = db.HashGetAll(key);

            var results = new Dictionary<string, T>();

            foreach (var hashEntry in hashEntries)
            {
                var field = hashEntry.Name;
                var value = hashEntry.Value;

                if (value.HasValue)
                {
                    var deserializedValue = JsonSerializer.Deserialize<T>(value);
                    results.Add(field, deserializedValue);
                }
            }

            return results;
        }
        /// <summary>
        /// The LSet method is a generic method that takes a key, an index, and a value as parameters to set the value at the specified index in a list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="index"></param>
        /// <param name="value"></param>
        public void LSet<T>(string key, long index, T value, ExpirationType expirationType = ExpirationType.Absolute, int seconds = defaultSeconds)
        {
            var db = _cache.GetDatabase();
            var serializedValue = JsonSerializer.Serialize(value);
            db.ListSetByIndex(key, index, serializedValue);

            TimeSpan? expiry = GetTimeSpan(key, expirationType, seconds);
            db.KeyExpire(key, expiry);
        }
        /// <summary>
        /// The LIndex method is a generic method that takes a key and an index as parameters and returns the element at the specified index from a list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public T LIndex<T>(string key, long index)
        {
            var db = _cache.GetDatabase();
            var value = db.ListGetByIndex(key, index);
            if (value.HasValue)
            {
                return JsonSerializer.Deserialize<T>(value);
            }
            return default;
        }
        /// <summary>
        /// The ListRightPush method is a generic method that takes a key and a List<T> as parameters to set the elements of a list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="values"></param>
        public void ListRightPush<T>(string key, List<T> values, ExpirationType expirationType = ExpirationType.Absolute, int seconds = defaultSeconds)
        {
            var db = _cache.GetDatabase();
            var serializedValues = values.Select<T, RedisValue>(x => JsonSerializer.Serialize(x)).ToArray();
            db.ListRightPush(key, serializedValues);

            TimeSpan? expiry = GetTimeSpan(key, expirationType, seconds);
            db.KeyExpire(key, expiry);
        }
        /// <summary>
        /// The SAdd method is a generic method that takes a key and a List<T> as parameters to add members to a set.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="members"></param>
        public void SAdd<T>(string key, List<T> members, ExpirationType expirationType = ExpirationType.Absolute, int seconds = defaultSeconds)
        {
            var db = _cache.GetDatabase();
            var serializedMembers = members.Select<T, RedisValue>(x => JsonSerializer.Serialize(x)).ToArray();
            db.SetAdd(key, serializedMembers);

            if (seconds == -1)
            {
                db.KeyPersist(key);
            }
            else
            {
                TimeSpan? expiry = GetTimeSpan(key, expirationType, seconds);
                db.KeyExpire(key, expiry);
            }
        }
        public void SAdd<T>(string key, T member, ExpirationType expirationType = ExpirationType.Absolute, int seconds = defaultSeconds)
        {
            var db = _cache.GetDatabase();
            var serializedMember = JsonSerializer.Serialize(member);
            db.SetAdd(key, serializedMember);

            if (seconds == -1)
            {
                db.KeyPersist(key);
            }
            else
            {
                TimeSpan? expiry = GetTimeSpan(key, expirationType, seconds);
                db.KeyExpire(key, expiry);
            }
        }

        /// <summary>
        /// The SMembers method is a generic method that takes a key as a parameter and returns a list of all members of a set.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public List<T> SMembers<T>(string key)
        {
            var db = _cache.GetDatabase();
            var members = db.SetMembers(key);

            var result = new List<T>();
            foreach (var member in members)
            {
                var deserializedMember = JsonSerializer.Deserialize<T>(member);
                result.Add(deserializedMember);
            }

            return result;
        }
        /// <summary>
        /// The LRange method is a generic method that takes a key, a start index, and a stop index as parameters and returns a range of elements from a list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <returns></returns>
        public List<T> LRange<T>(string key, long start, long stop)
        {
            var db = _cache.GetDatabase();
            var values = db.ListRange(key, start, stop);

            var result = new List<T>();
            foreach (var value in values)
            {
                var deserializedValue = JsonSerializer.Deserialize<T>(value);
                result.Add(deserializedValue);
            }

            return result;
        }
        public void Remove(string key)
        {
            var db = _cache.GetDatabase();
            db.KeyDelete(key);
        }
        public void InvalidateCache(string cacheKey)
        {
            Remove(cacheKey);
        }
        public void InvalidateCache(string invalidateKeyType, string cacheKey)
        {
            string keyPattern = string.Empty;
            switch (invalidateKeyType)
            {
                case InvalidateKeyType.PREFIX:
                    keyPattern = $"{cacheKey}*";
                    break;
                case InvalidateKeyType.SUFFIX:
                    keyPattern = $"*{cacheKey}";
                    break;
                case InvalidateKeyType.LIKE:
                    keyPattern = $"*{cacheKey}*";
                    break;
                default:
                    keyPattern = $"{cacheKey}";
                    break;
            }
            InvalidateCacheByPattern(keyPattern);
        }
        private void InvalidateCacheByPattern(string keyPattern)
        {
            //var endpoints = _cache.GetEndPoints();
            //var server = _cache.GetServer(endpoints.First());

            //var keysToRemove = new List<string>();
            //var keys = server.Keys(pattern: keyPattern);
            //foreach (var key in keys)
            //{
            //    keysToRemove.Add(key);
            //}
            List<string> keysToRemove = GetKeyByPattern(keyPattern);
            foreach (var key in keysToRemove)
            {
                InvalidateCache(key);
            }
        }
        public List<string> GetKeyByPattern(string keyPattern)
        {
            var endpoints = _cache.GetEndPoints();
            var server = _cache.GetServer(endpoints.First());

            var keysToReturn = new List<string>();
            var keys = server.Keys(pattern: keyPattern);
            foreach (var key in keys)
            {
                keysToReturn.Add(key);
            }
            return keysToReturn;
        }

        //public void InvalidateCacheByPrefix1(string cacheKeyPrefix)
        //{
        //    var endpoints = _cache.GetEndPoints();
        //    var server = _cache.GetServer(endpoints.First());

        //    var keysToRemove = new List<string>();
        //    var keys = server.Keys(pattern: $"{cacheKeyPrefix}*");
        //    foreach (var key in keys)
        //    {
        //        keysToRemove.Add(key);
        //    }

        //    foreach (var key in keysToRemove)
        //    {
        //        InvalidateCache(key);
        //    }
        //}

    }
}
