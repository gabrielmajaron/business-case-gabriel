using System;
using System.Collections.Generic;
using Messenger.Configuration;
using System.Threading.Tasks;
using Messenger.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Messenger.EventBus.EventBusRabbitMQ.Configuration;
using Messenger.Extensions;

namespace Messenger.RabbitMQ
{
    public class RBMQConnection : IRBMQConnection
    {
        private ConnectionFactory _factory;
        private IConnection _connection;
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly IQueueNameBuilder _queueNameBuilder;
        private readonly bool _hasDelayRetry;

        private readonly Dictionary<string, IModel> _channels;

        public RBMQConnection(IConfiguration configuration, ILogger<RBMQConnection> logger, IQueueNameBuilder queueNameBuilder)
        {
            _channels = new Dictionary<string, IModel>();
            _configuration = configuration;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _queueNameBuilder = queueNameBuilder ?? throw new ArgumentNullException(nameof(queueNameBuilder));
            _hasDelayRetry = configuration.GetOptional<EventBusRabbitMQBaseSettings>(c => c.RABBITMQ_HAS_DELAY_RETRY);
            CreateConnection();
        }

        private void CreateConnection()
        {
            var virtualHost = _configuration.GetOptional<EventBusRabbitMQBaseSettings>(x => x.RABBITMQ_VIRTUAL_HOST);

            _factory = new ConnectionFactory
            {
                Uri = new Uri(_configuration.GetRequired<RabbitMQBaseSettings>(s => s.RABBITMQ_ENDPOINT)),
                Password = _configuration.GetRequired<RabbitMQBaseSettings>(s => s.RABBITMQ_PASSWORD),
                UserName = _configuration.GetRequired<RabbitMQBaseSettings>(s => s.RABBITMQ_USER),
                VirtualHost = virtualHost ?? "/",
                DispatchConsumersAsync = true
            };

            _connection = _factory.CreateConnection();
        }

        public IModel GetChannel(string name, string queueName)
        {
            IModel channel = null;
            if (!_channels.TryGetValue(name, out channel))
            {
                channel = _connection.CreateModel();
                try
                {
                    channel.QueueBind(
                        queue: queueName,
                        exchange: _queueNameBuilder.GetExchangeDefault(),
                        routingKey: queueName
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        "Error in queue bind - QueueName: {1} - ErrorMessage: {2}", 
                        queueName, 
                        ex.Message);
                    throw;
                }
                _channels.Add(name, channel);
            }

            return channel;
        }

        public void Consume(string name, Func<string, Task<int>> execute)
        {
            var queue = new RBMQQueue(this, _logger, _queueNameBuilder);
            
            queue.Consume(name, async (string message) =>
            {
                await execute(message);
                return 0;
            }, _hasDelayRetry);
        }
        
        public void Publish(string name, dynamic data)
        {
            var queue = new RBMQQueue(this, _logger,_queueNameBuilder);
            queue.Publish(name,data);
        }

        public void PublishList(string name, List<object> list)
        {
            var queue = new RBMQQueue(this, _logger,_queueNameBuilder);
            foreach (var data in list)
            {
                queue.Publish(name,data);
            }
        }
    }
}
