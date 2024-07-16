using System;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Messenger.Extensions;
using Messenger.Interfaces;
using Messenger.QueueItems;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Messenger.RabbitMQ
{
    public class RBMQQueue
    {
        private RBMQConnection _connection;
        private readonly ILogger _logger;
        private readonly IQueueNameBuilder _queueNameBuilder;

        public RBMQQueue(RBMQConnection connection, ILogger logger, IQueueNameBuilder queueNameBuilder)
        {
            _connection = connection;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _queueNameBuilder = queueNameBuilder ?? throw new ArgumentNullException(nameof(queueNameBuilder));
        }

        public void Consume(string name, Func<string, Task<int>> execute, bool isDelayRetry = false)
        {
            var queueName = _queueNameBuilder.BuildCompanyQueueName(name);
            var channel = _connection.GetChannel(name, queueName);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += async (_, ea) =>
            {
                RabbitMqMessage rabbitMqMessage = new RabbitMqMessage();
                try
                {
                    rabbitMqMessage = ConsumeMessage(ea);
                    
                    await execute(rabbitMqMessage?.Content);

                    channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(rabbitMqMessage.GetMsgLogError(), DateTime.Now, queueName, ex.Message);
                    
                    if (isDelayRetry)
                        if(PublishDelay(name, rabbitMqMessage))
                            channel.BasicAck(ea.DeliveryTag, false);
                        else
                            channel.BasicNack(ea.DeliveryTag, false, true);
                    else
                        channel.BasicNack(ea.DeliveryTag, false, true);
                }
            };
            channel.BasicConsume(queue: queueName,
                                 autoAck: false,
                                 consumer: consumer);
        }

        public void Publish(string name, dynamic data)
        {
            var queueName = _queueNameBuilder.BuildCompanyQueueName(name);
            var channel = _connection.GetChannel(name, queueName);

            RabbitMqMessage rabbitMqMessage = CreateMessage(data);
            
            var body = ConvertMessageToBytes(rabbitMqMessage);
            
            IBasicProperties props = channel.CreateBasicProperties();
            props.Persistent = true;

            channel.BasicPublish(exchange: _queueNameBuilder.GetExchangeDefault(),
                                 routingKey: queueName,
                                 basicProperties: props,
                                 body: body);
        }
        
        private bool PublishDelay(string name, RabbitMqMessage rabbitMqMessage)
        {
            try
            {
                var queueName = _queueNameBuilder.BuildCompanyDelayQueueName(name);
                var channel = _connection.GetChannel(queueName, queueName);
                
                _logger.LogInformation(rabbitMqMessage.GetMsgPublishDelay(), DateTime.Now, queueName);

                rabbitMqMessage.Attempts++;
                rabbitMqMessage.LastOrigin = Assembly.GetEntryAssembly()?.GetName().Name;
                
                var body = ConvertMessageToBytes(rabbitMqMessage);
                
                IBasicProperties props = channel.CreateBasicProperties();
                props.Persistent = true;
                props.Expiration = GetExpiration(rabbitMqMessage.Attempts);
            
                channel.BasicPublish(_queueNameBuilder.GetExchangeDefault(),
                    routingKey: queueName,
                    basicProperties: props,
                    body: body);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(rabbitMqMessage.GetMsgLogErrorPublishDelay(), DateTime.Now, ex.Message);
                return false;
            }
        }
        
        public string GetExpiration(int attempts)
        {
            int expiration = 0;
            switch (attempts)
            {
                case < 5:
                    expiration = 60000;
                    break;
                case < 10:
                    expiration = 300000;
                    break;
                case < 20:
                    expiration = 3600000;
                    break;
                default:
                    expiration = 86400000;
                    break;
            };

            return expiration.ToString();
        }

        private RabbitMqMessage CreateMessage(dynamic data)
        {
            return new RabbitMqMessage
            {
                Id = Guid.NewGuid().ToString(),
                Publication = DateTime.UtcNow.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss"),
                FirstOrigin = Assembly.GetEntryAssembly()?.GetName().Name,
                Attempts = 0,
                Content = JsonSerializer.Serialize(data)
            };
        }

        private RabbitMqMessage ConsumeMessage(BasicDeliverEventArgs basicDeliverEventArgs)
        {
            var body = basicDeliverEventArgs.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
                    
            return JsonSerializer.Deserialize<RabbitMqMessage>(message);
        }

        private byte[] ConvertMessageToBytes(RabbitMqMessage rabbitMqMessage)
        {
            string message = JsonSerializer.Serialize(rabbitMqMessage);
            return Encoding.UTF8.GetBytes(message);
        }
    }
}
