using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using WINITAPI.HostedServices;
using WINITServices.Interfaces.CacheHandler;
using WINITSharedObjects.Constants;

namespace WINITAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CacheController : WINITBaseController
    {
        public CacheController(IServiceProvider serviceProvider) 
            : base(serviceProvider)
        {
        }
        /// <summary>
        /// Call this method to start CacheService
        /// </summary>
        /// <returns></returns>

        [HttpGet] 
        [Route("InvalidateCacheAction")]
        public IActionResult InvalidateCacheAction(string invalidateKeyType, string cacheKey)
        {
            try
            {
                _cacheService.InvalidateCache(invalidateKeyType, cacheKey);
                return Ok("Cache invalidated successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest("Failed to invalidate cache: " + ex.Message);
            }
        }


    }
}