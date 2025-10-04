using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using WINITAPI.HostedServices;
using WINITServices.Classes.RabbitMQ;
using WINITServices.Interfaces.CacheHandler;
using WINITSharedObjects.Constants;

namespace WINITAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CacheManagerController : WINITBaseController
    {
        private readonly RabbitMQService _rabbitMQService;
        private readonly CacheHostedService _cacheHostedService;
        public CacheManagerController(ICacheService cacheService, RabbitMQService rabbitMQService, 
            CacheHostedService cacheHostedService) 
            : base(cacheService)
        {
            _rabbitMQService = rabbitMQService;
            _cacheHostedService = cacheHostedService;
        }
        /// <summary>
        /// Call this method to start CacheService
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("StartCacheService")]
        public IActionResult StartCacheService()
        {
            _cacheHostedService.StartAsync(default);
            return SendResponse("Service started successfully.");
        }

        /// <summary>
        /// Call this method to stop CacheService
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("StopCacheService")]
        public IActionResult StopCacheService()
        {
            _cacheHostedService.StopAsync(default);
            return SendResponse("Service stopped successfully.");
        }

        //[HttpGet]
        //[Route("InvalidateCache")]
        //public IActionResult InvalidateCache(string cacheKey)
        //{
        //    _rabbitMQService.SendMessage(RabbitMQQueueName.CACHE_INVALIDATE, cacheKey);
        //    return SendResponse("Cache invalidated successfully.");
        //}

        [HttpGet]
        [Route("InvalidateCache")]
        public IActionResult InvalidateCache(string invalidateKeyType, string cacheKey)
        {
            string message = JsonConvert.SerializeObject(new WINITSharedObjects.Models.RabbitMQ.CacheRabbitMQMessage { MessageType = invalidateKeyType, MessageText = cacheKey });
            _rabbitMQService.SendMessage(RabbitMQQueueName.CACHE_INVALIDATE, message);
            return SendResponse($"Cache invalidated successfully for key {cacheKey}.");
        }
    }
}