using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WINITServices.Classes.RabbitMQ
{
    public class RabbitMQService
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        public event Action<string> OnMessageReceived;
        private EventingBasicConsumer _consumer;
        private string _consumerTag;
        private readonly IConfiguration _configuration;

        public RabbitMQService(IConfiguration configuration)
        {
            _configuration = configuration;
            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMQ:HostName"], //"localhost", // Replace with your RabbitMQ server's hostname
                Port = Convert.ToInt32(_configuration["RabbitMQ:Port"]),//5672, // Replace with your RabbitMQ server's port number
                UserName = _configuration["RabbitMQ:UserName"],//"vishal", // Replace with your RabbitMQ server's login username
                Password = _configuration["RabbitMQ:Password"] //"XXXXX" // Replace with your RabbitMQ server's login password
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            SubscribeQueue(RabbitMQQueueName.CACHE_INVALIDATE);
        }
        public void SubscribeQueue(string queueName)
        {
            _channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
        }
        public void UnSubscribeQueue(string queueName)
        {
            _channel.QueueDelete(queueName, false, false);
        }
        public void SendMessage(string queueName, string message)
        {
            //_channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            var body = Encoding.UTF8.GetBytes(message);
            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            _channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: properties, body: body);
        }

        public void ConsumeMessage(string queueName/*, Action<string> messageHandler*/)
        {
            _consumer = new EventingBasicConsumer(_channel);
            _consumer.Received += Consumer_Received;
            _consumerTag = _channel.BasicConsume(queue: queueName, autoAck: false, consumer: _consumer);
        }
        // Method to stop consuming messages
        public void StopConsumingMessages()
        {
            if (_consumer != null)
            {
                _consumer.Received -= Consumer_Received;
                _channel.BasicCancel(_consumerTag);
                _consumer = null;
            }
        }
        private void Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            var body = e.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            if (OnMessageReceived != null)
            {
                OnMessageReceived?.Invoke(message);
                _channel.BasicAck(deliveryTag: e.DeliveryTag, multiple: false);
            }
        }
        public void Dispose()
        {
            _channel.Dispose();
            _connection.Dispose();
        }
    }
}