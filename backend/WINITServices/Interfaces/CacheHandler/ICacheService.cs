using System;
using System.Collections.Generic;
using WINITServices.Classes.CacheHandler;

namespace WINITServices.Interfaces.CacheHandler
{
    public interface ICacheService
    {
        T Get<T>(string key);
        void Set<T>(string key, T value, ExpirationType expirationType, int seconds);
        Dictionary<string, T> GetMultiple<T>(List<string> keys);
        void HSet<T>(string key, string field, T value, ExpirationType expirationType = ExpirationType.Absolute, int seconds = 0);
        void SetKeyExpirationTime(string key, ExpirationType expirationType = ExpirationType.Absolute, int seconds = -1);
        T HGet<T>(string key, string field);
        public List<T> HGet<T>(string key, List<string> fields);
        Dictionary<string, T> HGetAll<T>(string key);
        void LSet<T>(string key, long index, T value, ExpirationType expirationType = ExpirationType.Absolute, int seconds = 0);
        T LIndex<T>(string key, long index);
        void ListRightPush<T>(string key, List<T> values, ExpirationType expirationType = ExpirationType.Absolute, int seconds = 0);
        void SAdd<T>(string key, List<T> members, ExpirationType expirationType = ExpirationType.Absolute, int seconds = 0);
        void SAdd<T>(string key, T member, ExpirationType expirationType = ExpirationType.Absolute, int seconds = 0);
        List<T> SMembers<T>(string key);
        List<T> LRange<T>(string key, long start, long stop);
        void Remove(string key);
        void InvalidateCache(string cacheKey);
        void InvalidateCache(string invalidateKeyType, string cacheKey);
        List<string> GetKeyByPattern(string keyPattern);
        //void SetSortedSetAddAsync<T>(string key, T value, double score);
        //List<T> GetSortedSetAddAsync<T>(string key, string sortingPattern, double score);
        public List<string> GetHashValueByPattern(string key, string keyPattern);
    }
}