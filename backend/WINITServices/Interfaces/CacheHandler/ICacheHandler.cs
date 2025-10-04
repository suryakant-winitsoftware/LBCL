using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WINITServices.Interfaces
{
    public interface ICacheHandler
    {
        Task<int> AddCacheToken();
        Task<int> UpdateCacheToken();
        Task<int> DeleteCacheToken();
        Task<IEnumerable<WINITSharedObjects.Models.CacheToken>> SelectAllCacheTokens();
        Task<IEnumerable<WINITSharedObjects.Models.CacheToken>> SelectAllCacheTokenByLastModifiedTime(DateTime lastModifiedTime);
        Task<WINITSharedObjects.Models.CacheToken> SelectCacheTokenByCacheKey(string cacheKey);
    }
}
