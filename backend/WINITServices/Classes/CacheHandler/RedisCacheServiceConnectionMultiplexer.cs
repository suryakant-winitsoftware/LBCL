using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using WINITServices.Interfaces.CacheHandler;
using WINITSharedObjects.Constants;

namespace WINITServices.Classes.CacheHandler
{
    public class RedisCacheServiceConnectionMultiplexer : ICacheService
    {
        private static IDatabase _db;
        private readonly ConnectionMultiplexer _cache;
       // private const int defaultSeconds = 600;
        private const int defaultSeconds = Int32.MaxValue / 1000; // Approximately 68 years
        public readonly IServiceProvider _serviceProvider;
        private readonly JsonSerializerSettings _serializerSettings;
        private readonly IConfiguration _configuration;


        public RedisCacheServiceConnectionMultiplexer(ConnectionMultiplexer cache, IServiceProvider serviceProvider, JsonSerializerSettings serializerSettings, IConfiguration configuration)
        {
            _cache = cache;
            _serviceProvider = serviceProvider;
            _serializerSettings = serializerSettings;
            _configuration = configuration;
            _db = _cache.GetDatabase(Convert.ToInt32(_configuration["RedisCache:DBName"]));
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
            //var db = _cache.GetDatabase();
            TimeSpan? expiry = null;

            if (expirationType == ExpirationType.Absolute)
            {
                expiry = TimeSpan.FromSeconds(seconds);
            }
            else if (expirationType == ExpirationType.Sliding)
            {
                var existingTtl = _db.KeyTimeToLive(key);
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
            //var db = _cache.GetDatabase();
            var value = _db.StringGet(key);
            if (value.HasValue)
            {
                if (typeof(T) == typeof(string))
                {
                    // If T is string, return the value as-is
                    return (T)(object)value.ToString();
                }
                else
                {
                    return JsonConvert.DeserializeObject<T>(value, _serializerSettings);
                }
            }
            return default(T);
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
            //var db = _cache.GetDatabase();
            RedisValue redisValue;
            if (typeof(T) == typeof(string) || typeof(T) == typeof(int) || typeof(T) == typeof(Int64))
            {
                redisValue = value.ToString();
            }
            else
            {
                redisValue = JsonConvert.SerializeObject(value);
            }
            if (seconds == -1)
            {
                _db.KeyPersist(key);
                _db.StringSet(key, redisValue, null);
            }
            else
            {
                TimeSpan? expiry = GetTimeSpan(key, expirationType, seconds);
                _db.StringSet(key, redisValue, expiry);
            }
        }
        /// <summary>
        /// The GetMultiple method is a generic method that takes a list of keys (List<string> keys) and returns a dictionary with the key-value pairs.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keys"></param>
        /// <returns></returns>
        public Dictionary<string, T> GetMultiple<T>(List<string> keys)
        {
            //var db = _cache.GetDatabase();
            RedisKey[] redisKeys = keys.Select(k => (RedisKey)k).ToArray();
            RedisValue[] redisValues = _db.StringGet(redisKeys);
            var results = new Dictionary<string, T>();

            for (int i = 0; i < redisValues.Length; i++)
            {
                var key = keys[i];
                var value = redisValues[i];

                if (value.HasValue)
                {
                    if (typeof(T) == typeof(string))
                    {
                        // If T is string, add the value as-is
                        results.Add(key, (T)(object)value.ToString());
                    }
                    else
                    {
                        var deserializedValue = JsonConvert.DeserializeObject<T>(value, _serializerSettings);
                        results.Add(key, deserializedValue);
                    }
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
            //var db = _cache.GetDatabase();
            var serializedValue = JsonConvert.SerializeObject(value);
            _db.HashSet(key, field, serializedValue);

            //TimeSpan? expiry = GetTimeSpan(key, expirationType, seconds);
            //db.KeyExpire(key, expiry);
        }
        public void HSetDirect(string key, string field, string value, ExpirationType expirationType = ExpirationType.Absolute, int seconds = defaultSeconds)
        {
            //var db = _cache.GetDatabase();
            //var serializedValue = JsonConvert.SerializeObject(value);
            _db.HashSet(key, field, value);

            //TimeSpan? expiry = GetTimeSpan(key, expirationType, seconds);
            //db.KeyExpire(key, expiry);
        }
        public void SetKeyExpirationTime(string key, ExpirationType expirationType = ExpirationType.Absolute, int seconds = defaultSeconds)
        {
            if (seconds == -1)
            {
                _db.KeyPersist(key);
            }
            else
            {
                TimeSpan? expiry = GetTimeSpan(key, expirationType, seconds);
                _db.KeyExpire(key, expiry);
            }
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
            //var db = _cache.GetDatabase();
            var value = _db.HashGet(key, field);
            if (value.HasValue)
            {
                if (typeof(T) == typeof(string))
                {
                    // If T is string, return the value as-is
                    return (T)(object)value.ToString();
                }
                else
                {
                    return JsonConvert.DeserializeObject<T>(value, _serializerSettings);
                }
            }
            return default(T);
        }
        public List<T> HGet<T>(string key, List<string> fields)
        {
            var values = new List<T>();
            var hashEntries = _db.HashGet(key, fields.Select(f => (RedisValue)f).ToArray());

            foreach (var hashEntry in hashEntries)
            {
                if (hashEntry.HasValue)
                {
                    if (typeof(T) == typeof(string))
                    {
                        // If T is string, return the value as-is
                        values.Add((T)(object)hashEntry.ToString());
                    }
                    else
                    {
                        values.Add(JsonConvert.DeserializeObject<T>(hashEntry.ToString(), _serializerSettings));
                    }
                }
                //else
                //{
                //    values.Add(null);
                //}
            }
            return values;
        }

        /*
        public string HGetDirect(string key, string field)
        {
            //var db = _cache.GetDatabase();
            var value = _db.HashGet(key, field);
            if (value.HasValue)
            {
                return value;
            }
            return string.Empty;
        }
        */
        /// <summary>
        /// The HGetAll method is a generic method that takes a key as a parameter and returns a dictionary with the field-value pairs from the hash.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public Dictionary<string, T> HGetAll<T>(string key)
        {
            //var db = _cache.GetDatabase();
            var hashEntries = _db.HashGetAll(key);


            var results = new Dictionary<string, T>();

            foreach (var hashEntry in hashEntries)
            {
                var field = hashEntry.Name;
                var value = hashEntry.Value;

                if (typeof(T) == typeof(string))
                {
                    // If T is string, no need to deserialize, just use the value as-is
                    results.Add(field, (T)(object)value);
                }
                else
                {
                    //Type t = _serviceProvider.GetRequiredService<T>().GetType();
                    var deserializedValue = JsonConvert.DeserializeObject<T>(value, _serializerSettings);
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
            //var db = _cache.GetDatabase();
            var serializedValue = JsonConvert.SerializeObject(value);
            _db.ListSetByIndex(key, index, serializedValue);

            //TimeSpan? expiry = GetTimeSpan(key, expirationType, seconds);
            //_db.KeyExpire(key, expiry);
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
            //var db = _cache.GetDatabase();
            var value = _db.ListGetByIndex(key, index);
            if (value.HasValue)
            {
                if (typeof(T) == typeof(string))
                {
                    // If T is string, return the value as-is
                    return (T)(object)value;
                }
                else
                {
                    return JsonConvert.DeserializeObject<T>(value, _serializerSettings);
                }
            }
            return default(T);
        }
        /// <summary>
        /// The ListRightPush method is a generic method that takes a key and a List<T> as parameters to set the elements of a list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="values"></param>
        public void ListRightPush<T>(string key, List<T> values, ExpirationType expirationType = ExpirationType.Absolute, int seconds = defaultSeconds)
        {
            //var db = _cache.GetDatabase();
            var serializedValues = values.Select<T, RedisValue>(x => JsonConvert.SerializeObject(x)).ToArray();
            _db.ListRightPush(key, serializedValues);

            //TimeSpan? expiry = GetTimeSpan(key, expirationType, seconds);
            //db.KeyExpire(key, expiry);
        }
        /// <summary>
        /// The SAdd method is a generic method that takes a key and a List<T> as parameters to add members to a set.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="members"></param>
        public void SAdd<T>(string key, List<T> members, ExpirationType expirationType = ExpirationType.Absolute, int seconds = defaultSeconds)
        {
            //var db = _cache.GetDatabase();
            //var serializedMembers = members.Select<T, RedisValue>(x => JsonConvert.SerializeObject(x)).ToArray();

            //var serializedMembers = members.Select(x =>
            //{
            //    if (x is string || x is int || x is Int64)
            //        return (RedisValue)(object)x; // Avoid serialization for strings
            //    else
            //        return (RedisValue)JsonConvert.SerializeObject(x);
            //}).ToArray();

            var redisValues = new RedisValue[members.Count];

            for (int i = 0; i < members.Count; i++)
            {
                if (members[i] is int || members[i] is long)
                {
                    redisValues[i] = members[i].ToString(); // Directly add integer values as strings
                }
                else
                {
                    redisValues[i] = JsonConvert.SerializeObject(members[i]); // Serialize other types
                }
            }

            _db.SetAdd(key, redisValues);

            if (seconds == -1)
            {
                _db.KeyPersist(key);
            }
            else
            {
                TimeSpan? expiry = GetTimeSpan(key, expirationType, seconds);
                _db.KeyExpire(key, expiry);
            }
        }
        public void SAdd<T>(string key, T member, ExpirationType expirationType = ExpirationType.Absolute, int seconds = defaultSeconds)
        {
            //var db = _cache.GetDatabase();
            //var serializedMember = JsonConvert.SerializeObject(member);
            RedisValue serializedMember;

            if (typeof(T) == typeof(string) || typeof(T) == typeof(int) || typeof(T) == typeof(Int64))
            {
                serializedMember = member.ToString();
            }
            else
            {
                serializedMember = JsonConvert.SerializeObject(member);
            }

            _db.SetAdd(key, serializedMember);

            if (seconds == -1)
            {
                _db.KeyPersist(key);
            }
            else
            {
                TimeSpan? expiry = GetTimeSpan(key, expirationType, seconds);
                _db.KeyExpire(key, expiry);
            }
        }
        public void SAddRedisValue(string key, RedisValue[] redisValues, ExpirationType expirationType = ExpirationType.Absolute, int seconds = defaultSeconds)
        {
            _db.SetAdd(key, redisValues);

            if (seconds == -1)
            {
                _db.KeyPersist(key);
            }
            else
            {
                TimeSpan? expiry = GetTimeSpan(key, expirationType, seconds);
                _db.KeyExpire(key, expiry);
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
            //var db = _cache.GetDatabase();
            var members = _db.SetMembers(key);

            var result = new List<T>();
            foreach (var member in members)
            {
                if (typeof(T) == typeof(string) || typeof(T) == typeof(int) || typeof(T) == typeof(Int64))
                {
                    // If T is string, add the member as-is
                    //result.Add((T)(object)member);
                    result.Add((T)Convert.ChangeType(member, typeof(T)));
                }
                else
                {
                    var deserializedMember = JsonConvert.DeserializeObject<T>(member, _serializerSettings);
                    result.Add(deserializedMember);
                }
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
            //var db = _cache.GetDatabase();
            var values = _db.ListRange(key, start, stop);

            var result = new List<T>();
            foreach (var value in values)
            {
                if (typeof(T) == typeof(string))
                {
                    // If T is string, add the member as-is
                    result.Add((T)(object)value);
                }
                else
                {
                    var deserializedMember = JsonConvert.DeserializeObject<T>(value, _serializerSettings);
                    result.Add(deserializedMember);
                }
            }

            return result;
        }
        public List<T> GetCombineValues<T>(RedisKey[] setNames, SetOperation setOperation)
        {
            RedisValue[] intersectedValues = _db.SetCombine(setOperation, setNames);
            List<T> result = intersectedValues.Select(rv => (T)Convert.ChangeType(rv, typeof(T))).ToList();
            return result;
        }

        public RedisValue[] GetCombineRedisValues(RedisKey[] setNames, SetOperation setOperation)
        {
            return _db.SetCombine(setOperation, setNames);
        }

        public void SetCombineAndStore(SetOperation operation, RedisKey destination, RedisKey[] keys, CommandFlags flags = CommandFlags.None)
        {
            _db.SetCombineAndStore(operation, destination, keys, flags);
        }
        public void SetCombineAndStore1(SetOperation operation, RedisKey destination, RedisKey firstKey, RedisKey secondKey, CommandFlags flags = CommandFlags.None)
        {
            _db.SetCombineAndStore(operation, destination, firstKey, secondKey, flags);
        }

        public void Remove(string key)
        {
            //var db = _cache.GetDatabase();
            _db.KeyDelete(key);
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
                    keyPattern = string.Format("{0}*", cacheKey);
                    break;
                case InvalidateKeyType.SUFFIX:
                    keyPattern = string.Format("*{0}", cacheKey);
                    break;
                case InvalidateKeyType.LIKE:
                    keyPattern = string.Format("*{0}*", cacheKey);
                    break;
                default:
                    keyPattern = string.Format("{0}", cacheKey);
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
            var keys = server.Keys(database: (_db.Database), pattern: keyPattern);
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

        public void SetSortedSetAddAsync<T>(string key, T value, double score)
        {
            string redisValue = string.Empty;
            if (typeof(T) == typeof(string) || typeof(T) == typeof(int) || typeof(T) == typeof(Int64))
            {
                redisValue = value.ToString();
            }
            else
            {
                redisValue = JsonConvert.SerializeObject(value);
            }
            _db.SortedSetAddAsync(key, redisValue, score);
        }
        public List<T> GetSortedSetAddAsync<T>(string key, string sortingPattern, double score)
        {
            var sortedItems = _db.Sort(key,
            by: sortingPattern,
            flags: CommandFlags.None);

            var result = new List<T>();
            foreach (var value in sortedItems)
            {
                if (typeof(T) == typeof(string) || typeof(T) == typeof(int) || typeof(T) == typeof(Int64))
                {
                    // If T is string, add the member as-is
                    result.Add((T)(object)value);
                }
                else
                {
                    var deserializedMember = JsonConvert.DeserializeObject<T>(value, _serializerSettings);
                    result.Add(deserializedMember);
                }
            }
            return result;
        }
        public List<string> GetHashValueByPattern(string key, string keyPattern)
        {
            List<string> selectedValues = new List<string>();
            IEnumerable<HashEntry>  hashEntries = _db.HashScan(key, keyPattern, int.MaxValue);

            // Process the scanned elements (StoreNames and corresponding StoreCodes)
            foreach (var entry in hashEntries)
            {
                selectedValues.Add(entry.Value);
                //string storeName = entry.Name;
                //string storeCode = entry.Value;
            }
            return selectedValues;
        }
        //public void Test()
        //{
        //    string key = "myHash"; // Replace with your Hash key
        //    string pattern = "field*"; // Replace with your desired pattern

        //    long cursor = 0;
        //    const int pageSize = 10; // Adjust the page size as needed

        //    do
        //    {
        //        var scanResult = _db.HashScan(key, pattern, pageSize, cursor);
        //        foreach (var entry in scanResult)
        //        {
        //            Console.WriteLine($"Field Name: {entry.Name}");
        //        }

        //        cursor = scanResult.Cont
        //    } while (cursor != 0);
        //}

    }
}
