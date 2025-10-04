using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WINITServices.Classes.RabbitMQ;
using WINITSharedObjects.Models.RabbitMQ;
using WINITServices.Interfaces.CacheHandler;

namespace WINITAPI.HostedServices
{
    public class CacheHostedService : IHostedService
    {
        private readonly ICacheService _cacheService;
        private readonly WINITServices.Classes.RabbitMQ.RabbitMQService _rabbitMQService;
        public CacheHostedService(ICacheService cacheService, WINITServices.Classes.RabbitMQ.RabbitMQService rabbitMQService)
        {
            _cacheService = cacheService;
            _rabbitMQService = rabbitMQService;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            //_rabbitMQService.SubscribeQueue(RabbitMQQueueName.CACHE_INVALIDATE);
            _rabbitMQService.OnMessageReceived += _rabbitMQService_OnMessageReceived;
            _rabbitMQService.ConsumeMessage(RabbitMQQueueName.CACHE_INVALIDATE);
            return Task.CompletedTask;
        }

        //private void _rabbitMQService_OnMessageReceived(string obj)
        //{
        //    _cacheService.InvalidateCache(obj);
        //}

        private void _rabbitMQService_OnMessageReceived(string obj)
        {
            CacheRabbitMQMessage cacheRabbitMQMessage = JsonConvert.DeserializeObject<CacheRabbitMQMessage>(obj);
            _cacheService.InvalidateCache(cacheRabbitMQMessage.MessageType, cacheRabbitMQMessage.MessageText);
            //CacheCommonFunctions.DebugWriteLine("Message received from RabbitMQ as:" + obj);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _rabbitMQService.OnMessageReceived -= _rabbitMQService_OnMessageReceived;
            _rabbitMQService.StopConsumingMessages();
            return Task.CompletedTask;
        }
    }
}