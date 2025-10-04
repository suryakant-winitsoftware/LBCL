using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using WINITSharedObjects.Models.RuleEngine;

namespace WINITServices.Classes.RabbitMQ
{
    public class SubscriberMQService : IDisposable
    {
        private readonly ConnectionFactory _factory;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly IConfiguration _configuration;
        private readonly string queueName;
        public event Action<MessageData> OnMessageReceived;

        public SubscriberMQService(IConfiguration configuration)
        {
            _configuration = configuration;
            _factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMQ:HostName"], //"localhost", // Replace with your RabbitMQ server's hostname
                Port = Convert.ToInt32(_configuration["RabbitMQ:Port"]),//5672, // Replace with your RabbitMQ server's port number
                UserName = _configuration["RabbitMQ:UserName"],//"guest", // Replace with your RabbitMQ server's login username
                Password = _configuration["RabbitMQ:Password"] //"guest" // Replace with your RabbitMQ server's login password
            };
            queueName = RabbitMQQueueName.RuleEngineQ;
            try
            {
                _connection = _factory.CreateConnection();
                _channel = _connection.CreateModel();
            }
            catch (Exception ex)
            {
                // Handle connection creation error
                Console.WriteLine("Error creating RabbitMQ connection: " + ex.Message);
                throw;
            }
        }
        public void StartConsuming(Action<MessageData> messageHandler)
        {
            var _consumer = new EventingBasicConsumer(_channel);
            this.OnMessageReceived = messageHandler;
            _consumer.Received += Consumer_Received;
             _channel.BasicConsume(queueName, false, _consumer);
        }
        private void Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            var body = e.Body.ToArray();
            var serializedData = Encoding.UTF8.GetString(body);
            var data = JsonConvert.DeserializeObject<MessageData>(serializedData);
            if (OnMessageReceived != null)
            {
                OnMessageReceived?.Invoke(data);
                _channel.BasicAck(deliveryTag: e.DeliveryTag, multiple: false);
            }
        }
        public void StartConsuming(Func<MessageData, bool> messageHandler)
        {
            try
            {
                _channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
                var consumer = new EventingBasicConsumer(_channel);
                consumer.Received += (sender, args) =>
                {
                    byte[] body = args.Body.ToArray();
                    var serializedData = Encoding.UTF8.GetString(body);
                    Console.WriteLine("Received message: " + serializedData);
                    var message = JsonConvert.DeserializeObject<MessageData>(serializedData);
                    bool isHandled = messageHandler.Invoke(message);
                    if (isHandled)
                    {
                        //Console.WriteLine("Message processed: {0}", message);
                        _channel.BasicAck(args.DeliveryTag, false);
                    }
                    else
                    {
                        // Console.WriteLine("Message processing failed: {0}", message);
                        // Perform appropriate error handling or rejection logic here
                    }
                   

                    // Process the message as needed

                    //_channel.BasicAck(args.DeliveryTag, multiple: false); // Acknowledge the message
                };

                _channel.BasicConsume(queueName, false, consumer); // Set autoAck to false
            }
            catch (Exception ex)
            {
                // Handle message consumption error
                Console.WriteLine("Error consuming message: " + ex.Message);
                throw;
            }
        }

        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }

}
