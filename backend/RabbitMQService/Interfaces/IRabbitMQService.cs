using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQService.Interfaces
{
    public interface IRabbitMQService
    {

        void SubscribeQueue(string queueName, Func<string, Task> onMessageReceived);

        void UnSubscribeQueue(string queueName);

        void SendMessage(string queueName, string message);

        void ConsumeMessage(string queueName);

        Task Consumer_ReceivedAsync(string queueName, BasicDeliverEventArgs e);

        int GetMessageRetryCount(IBasicProperties properties);

        void SetMessageRetryCount(IBasicProperties properties, int retryCount);
        void EnsureChannelCreated();
    }
}
