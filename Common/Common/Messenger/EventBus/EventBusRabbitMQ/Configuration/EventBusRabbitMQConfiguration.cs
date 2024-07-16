using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Messenger.EventBus.EventBus;
using Messenger.EventBus.EventBus.Abstractions;
using Messenger.Extensions;
using RabbitMQ.Client;

namespace Messenger.EventBus.EventBusRabbitMQ.Configuration
{
    public static class EventBusRabbitMQConfiguration
    {
        public static void AddEventBusRabbitMQ(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IRabbitMQPersistentConnection>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<DefaultRabbitMQPersistentConnection>>();

                var virtualHost = configuration.GetOptional<EventBusRabbitMQBaseSettings>(x => x.RABBITMQ_VIRTUAL_HOST);

                var factory = new ConnectionFactory
                {
                    Uri = new Uri(configuration.GetRequired<EventBusRabbitMQBaseSettings>(x => x.RABBITMQ_ENDPOINT)),
                    UserName = configuration.GetRequired<EventBusRabbitMQBaseSettings>(x => x.RABBITMQ_USER),
                    Password = configuration.GetRequired<EventBusRabbitMQBaseSettings>(x => x.RABBITMQ_PASSWORD),
                    VirtualHost = virtualHost ?? "/",
                    DispatchConsumersAsync = true
                };

                var retryCount = GetRetryConnectionCount(configuration);

                return new DefaultRabbitMQPersistentConnection(factory, logger, retryCount);
            });

            services.AddSingleton<IEventBus, EventBusRabbitMQ>(serviceProvider =>
            {
                var rabbitMQPersistentConnection = serviceProvider.GetRequiredService<IRabbitMQPersistentConnection>();
                var logger = serviceProvider.GetRequiredService<ILogger<EventBusRabbitMQ>>();
                var eventBusSubcriptionsManager = serviceProvider.GetRequiredService<IEventBusSubscriptionsManager>();

                var brokerName = configuration.GetRequired<EventBusRabbitMQBaseSettings>(x => x.RABBITMQ_BROKER);
                var subscriptionQueueName = configuration.GetOptional<EventBusRabbitMQBaseSettings>(x => x.RABBITMQ_QUEUE);
                var hasRetry = GetHasRetry(configuration);
                var retryConnectionCount = GetRetryConnectionCount(configuration);

                return new EventBusRabbitMQ(
                    rabbitMQPersistentConnection, logger, serviceProvider, eventBusSubcriptionsManager,
                    brokerName, subscriptionQueueName, retryConnectionCount, hasRetry);
            });
            
            services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();
        }

        private static int GetRetryConnectionCount(IConfiguration configuration)
        {
            var retryConfig = configuration.GetOptional<EventBusRabbitMQBaseSettings>(x => x.RABBITMQ_CONN_RETRIES);
            var retryCount = 5;

            if (!string.IsNullOrEmpty(retryConfig))
                retryCount = int.Parse(retryConfig);

            return retryCount;
        }
        
        private static bool GetHasRetry(IConfiguration configuration)
        {
            return configuration.GetOptional<EventBusRabbitMQBaseSettings>(x => x.RABBITMQ_HAS_DELAY_RETRY);
            /*var retryConfig = configuration.GetOptional<EventBusRabbitMQBaseSettings>(x => x.RABBIT_HAS_RETRY);

            var hasRetry = false;
            
            if (!string.IsNullOrEmpty(retryConfig))
                hasRetry = bool.Parse(retryConfig);

            return hasRetry;*/
        }
    }
}