using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System;
using System.Text;

namespace WINITServices.Classes.RabbitMQ
{
    public class PublisherMQService : IDisposable
    {
        private readonly ConnectionFactory _factory;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly IConfiguration _configuration;
        private readonly string queueName;
        public PublisherMQService(IConfiguration configuration)
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

        public void PublishMessage(string message)
        {
            try
            {
                _channel.QueueDeclare(queueName, durable: true, false, false, null);
                var body = Encoding.UTF8.GetBytes(message);

                var properties = _channel.CreateBasicProperties();
                properties.Persistent = true; // Set message delivery mode to persistent

                _channel.BasicPublish("", queueName, properties, body);
            }
            catch (Exception ex)
            {
                // Handle message publishing error
                Console.WriteLine("Error publishing message: " + ex.Message);
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
