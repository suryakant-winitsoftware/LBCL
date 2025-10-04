using System.Text;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using DBServices.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WINIT.Shared.Models.Models;
using RabbitMQService.Interfaces;

namespace RabbitMQService.Classes
{
    public class RabbitMQService : IRabbitMQService, IDisposable
    {
        private readonly IConnection _connection;
        private IModel _channel;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RabbitMQService> _logger;
        Dictionary<string, object> queueArgs = null;
        // Dictionary to hold events for each queue
        // private readonly Dictionary<string, Action<string>> _queueEvents;
        private readonly Dictionary<string, Func<string, Task>> _queueAsyncEvents;
        private readonly IDBService _dbService;
        int MaxRetries = 3;
        public RabbitMQService(IConfiguration configuration, ILogger<RabbitMQService> logger, IDBService dbService, IModel channel = null)
        {
            _configuration = configuration;
            _logger = logger;
            _channel = channel;

            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _configuration["RabbitMQ:HostName"],
                    Port = Convert.ToInt32(_configuration["RabbitMQ:Port"]),
                    UserName = _configuration["RabbitMQ:UserName"],
                    Password = _configuration["RabbitMQ:Password"]
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                // _queueEvents = new Dictionary<string, Action<string>>();
                _queueAsyncEvents = new Dictionary<string, Func<string, Task>>();
                _dbService = dbService;
                InitializeDeadLetterHandling();
                queueArgs = new Dictionary<string, object>
                {
                    { "x-dead-letter-exchange", "dlx" }
                };
            }
            catch (RabbitMQ.Client.Exceptions.BrokerUnreachableException ex)
            {
                _logger.LogError(ex, "Failed to connect to RabbitMQ server. Ensure that the server is running and the connection settings are correct.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while initializing RabbitMQ service.");
            }
        }
        private void InitializeDeadLetterHandling()
        {
            // Declare the Dead Letter Exchange
            _channel.ExchangeDeclare("dlx", ExchangeType.Fanout);

            // Declare the Dead Letter Queue
            _channel.QueueDeclare("dlq", durable: true, exclusive: false, autoDelete: false, arguments: null);

            // Bind the Dead Letter Queue to the Dead Letter Exchange
            _channel.QueueBind("dlq", "dlx", "");
        }
        //public void SubscribeQueue(string queueName, Action<string> onMessageReceived)
        //{
        //    _channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
        //    _queueEvents[queueName] = onMessageReceived;
        //}
        public void SubscribeQueue(string queueName, Func<string, Task> onMessageReceived)
        {
            try
            {
                _channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false, arguments: queueArgs);
                _queueAsyncEvents[queueName] = onMessageReceived;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while initializing RabbitMQ service.");
                throw;
            }
        }
            public void UnSubscribeQueue(string queueName)
        {
            _channel.QueueDelete(queueName, false, false);
            //if (_queueEvents.ContainsKey(queueName))
            //    _queueEvents.Remove(queueName);
            if (_queueAsyncEvents.ContainsKey(queueName))
                _queueAsyncEvents.Remove(queueName);
        }

        public void SendMessage(string queueName, string message)
        {
            try
            {
                EnsureChannelCreated();
                _channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false, arguments: queueArgs);
                var body = Encoding.UTF8.GetBytes(message);
                var properties = _channel.CreateBasicProperties();
                properties.Headers = new Dictionary<string, object>
                    {
                        { "x-retry-count", 1 } // Initial retry count
                    };
                properties.Persistent = true;
                _channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: properties, body: body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while sending the message.");
            }
        }

        public void ConsumeMessage(string queueName)
        {
            try
            {
                EnsureChannelCreated();
                _channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false, arguments: queueArgs);
                if (_channel.IsOpen)
                {
                    var consumer = new EventingBasicConsumer(_channel);
                    consumer.Received += async (sender, args) => await Consumer_ReceivedAsync(queueName, args);
                    _channel.BasicConsume(queueName, autoAck: false, consumer: consumer);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while consuming the message.");
            }
        }

        /*  private void Consumer_Received(string queueName, BasicDeliverEventArgs e)
          {
              try
              {
                  var body = e.Body.ToArray();
                  var message = Encoding.UTF8.GetString(body);
                  _queueEvents[queueName]?.Invoke(message);
                  _channel.BasicAck(deliveryTag: e.DeliveryTag, multiple: false);
              }
              catch (Exception ex)
              {
                  _logger.LogError(ex, "An error occurred while processing the message. The message will be re-queued.");
                  _channel.BasicNack(deliveryTag: e.DeliveryTag, multiple: false, requeue: true);
              }
          }*/
        public async Task Consumer_ReceivedAsync(string queueName, BasicDeliverEventArgs e)
        {
            MessageModel? messageModel = null;
            try
            {
                var body = e.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                messageModel = JsonConvert.DeserializeObject<MessageModel>(message);
                // Check if the message has a retry count header

                try
                {
                    // Attempt to process the message
                    await _queueAsyncEvents[queueName].Invoke(message);
                    _channel.BasicAck(deliveryTag: e.DeliveryTag, multiple: false);
                    //  _dBService.UpdateLogByStep(messegeModel.MessegeUID, "", false, true, false, "");
                }
                catch (Exception ex)
                {
                    int retryCount = GetMessageRetryCount(e.BasicProperties);
                    _logger.LogError(ex, "An error occurred while processing the message.");
                    // Increment the retry count
                    retryCount++;

                    if (retryCount <= MaxRetries)
                    {
                        // Set the retry count header and republish with a delay
                        SetMessageRetryCount(e.BasicProperties, retryCount);
                        _channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: e.BasicProperties, body: body);
                        var t = (int)Math.Pow(2, retryCount) * 1000;
                        _logger.LogInformation($"Message will be retried after {t} Seconds. Retry Count: {retryCount}");
                        await _dbService.UpdateLogByStepAsync(messageModel?.MessageUID ?? "", "", false, false, $"Message will be retried after {t} Seconds. Retry Count: {retryCount}");
                        // Wait before retrying (you can use exponential backoff)
                        Thread.Sleep(t);
                    }
                    else
                    {
                        await _dbService.UpdateLogByStepAsync(messageModel?.MessageUID ?? "", "", false, true, $"Max retries reached. Moving the message to the dead-letter queue.");
                        _logger.LogError($"Max retries reached. Moving the message to the dead-letter queue.");
                        _channel.BasicNack(deliveryTag: e.DeliveryTag, multiple: false, requeue: false);
                        // Optionally, you can move the message to a dead-letter queue
                        //MoveToDeadLetterQueue(queueName, e.BasicProperties, body);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred.");
                await _dbService.UpdateLogByStepAsync(messageModel?.MessageUID ?? "", "", false, true, ex.Message);
            }
        }

        public int GetMessageRetryCount(IBasicProperties properties)
        {
            // Get the retry count from the message headers
            return properties.Headers.ContainsKey("x-retry-count") ? (int)properties.Headers["x-retry-count"] : 0;
        }

        public void SetMessageRetryCount(IBasicProperties properties, int retryCount)
        {
            // Set the retry count in the message headers
            properties.Headers ??= new Dictionary<string, object>();
            properties.Headers["x-retry-count"] = retryCount;
        }

        private void MoveToDeadLetterQueue(string queueName, IBasicProperties properties, byte[] body)
        {
            // Move the message to the dead-letter exchange
            _channel.BasicPublish(exchange: "dlx", routingKey: "dlq", basicProperties: properties, body: body);
        }

        public void EnsureChannelCreated()
        {
            try
            {
                if (_channel == null || !_channel.IsOpen)
                {
                    _channel = _connection.CreateModel();
                }
            }catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating RabbitMQ channel.");
                throw;

            }
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}
