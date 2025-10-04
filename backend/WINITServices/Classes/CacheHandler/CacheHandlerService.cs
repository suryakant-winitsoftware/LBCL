using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WINITServices.Interfaces;
using WINITSharedObjects.Models;

namespace WINITServices.Classes.CacheHandler
{
    public class CacheHandlerService : ICacheHandler
    {
        protected readonly WINITRepository.Interfaces.CacheHandler.ICacheHandlerRepository _cacheHandlerRepository;
        public CacheHandlerService(WINITRepository.Interfaces.CacheHandler.ICacheHandlerRepository cacheHandlerRepository)
        {
            _cacheHandlerRepository = cacheHandlerRepository;
        }
        public async Task<int> AddCacheToken()
        {
            throw new NotImplementedException();
        }
        public async Task<int> UpdateCacheToken()
        {
            throw new NotImplementedException();
        }
        public async Task<int> DeleteCacheToken()
        {
            throw new NotImplementedException();
        }
        public async Task<IEnumerable<CacheToken>> SelectAllCacheTokens()
        {
            return await _cacheHandlerRepository.SelectAllCacheTokens();
        }
        public async Task<IEnumerable<CacheToken>> SelectAllCacheTokenByLastModifiedTime(DateTime lastModifiedTime)
        {
            return await _cacheHandlerRepository.SelectAllCacheTokenByLastModifiedTime(lastModifiedTime);
        }
        public async Task<CacheToken> SelectCacheTokenByCacheKey(string cacheKey)
        {
            return await _cacheHandlerRepository.SelectCacheTokenByCacheKey(cacheKey);
        }
    }
}
