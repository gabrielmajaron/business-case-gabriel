using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Messenger.EventBus.EventBus;
using Messenger.EventBus.EventBus.Abstractions;
using Messenger.EventBus.EventBus.Events;
using Messenger.EventBus.EventBus.Extensions;
using Messenger.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace Messenger.EventBus.EventBusRabbitMQ;

public class EventBusRabbitMQ : IEventBus, IDisposable
{
    private readonly IRabbitMQPersistentConnection _persistentConnection;
    private readonly ILogger<EventBusRabbitMQ> _logger;
    private readonly IEventBusSubscriptionsManager _subsManager;
    private readonly IServiceProvider _serviceProvider;
    private readonly int _retryConnectionCount;
    private readonly string _brokerName;
    private readonly bool _hasRetry;

    private IModel _consumerChannel;
    private string _queueName;

    public EventBusRabbitMQ(
        IRabbitMQPersistentConnection persistentConnection,
        ILogger<EventBusRabbitMQ> logger,
        IServiceProvider serviceProvider,
        IEventBusSubscriptionsManager subsManager,
        string brokerName = "paranabanco_event_bus",
        string queueName = null,
        int retryConnectionCount = 5,
        bool hasRetry = false)
    {
        _persistentConnection = persistentConnection ?? throw new ArgumentNullException(nameof(persistentConnection));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _subsManager = subsManager ?? new InMemoryEventBusSubscriptionsManager();
        _serviceProvider = serviceProvider;
        _brokerName = brokerName;
        _retryConnectionCount = retryConnectionCount;
        _hasRetry = hasRetry;

        if(!string.IsNullOrEmpty(queueName))
        {
            _queueName = queueName;
            _consumerChannel = CreateConsumerChannel();
            _subsManager.OnEventRemoved += SubsManager_OnEventRemoved;
        }
    }

    public bool IsConnected()
    {
        if (!_persistentConnection.IsConnected)
            _persistentConnection.TryConnect();

        return _persistentConnection.IsConnected;
    }

    private void SubsManager_OnEventRemoved(object sender, string eventName)
    {
        if (!_persistentConnection.IsConnected)
        {
            _persistentConnection.TryConnect();
        }

        using (var channel = _persistentConnection.CreateModel())
        {
            channel.QueueUnbind(queue: _queueName,
                exchange: _brokerName,
                routingKey: eventName);

            if (_subsManager.IsEmpty)
            {
                _queueName = string.Empty;
                _consumerChannel.Close();
            }
        }
    }

    public void Publish(IntegrationEvent @event)
    {
        if (!_persistentConnection.IsConnected)
        {
            _persistentConnection.TryConnect();
        }

        var policy = RetryPolicy.Handle<BrokerUnreachableException>()
            .Or<SocketException>()
            .WaitAndRetry(_retryConnectionCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
            {
                _logger.LogWarning(ex, "Could not publish event: {EventId} after {Timeout}s ({ExceptionMessage})", @event.Id, $"{time.TotalSeconds:n1}", ex.Message);
            });
        
        var eventName = @event.EventName.GetDescription();

        _logger.LogTrace("Creating RabbitMQ channel to publish event: {EventId} ({EventName})", @event.Id, eventName);

        using (var channel = _persistentConnection.CreateModel())
        {
            _logger.LogTrace("Declaring RabbitMQ exchange to publish event: {EventId}", @event.Id);

            channel.ExchangeDeclare(exchange: _brokerName, type: "direct", durable: true);

            var message = JsonConvert.SerializeObject(@event, 
                new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
            
            var body = Encoding.UTF8.GetBytes(message);

            policy.Execute(() =>
            {
                var properties = channel.CreateBasicProperties();
                properties.DeliveryMode = 2; // persistent

                _logger.LogTrace("Publishing event to RabbitMQ: {EventId}", @event.Id);

                channel.BasicPublish(
                    exchange: _brokerName,
                    routingKey: eventName,
                    mandatory: true,
                    basicProperties: properties,
                    body: body);
            });
        }
    }

    public void Subscribe<T, TH>()
        where T : IntegrationEvent
        where TH : IIntegrationEventHandler<T>
    {
        var eventName = _subsManager.GetEventKey<T>();
        DoInternalSubscription(eventName); // bind queue_name x routing_key

        _logger.LogInformation("Subscribing to event {EventName} with {EventHandler}", eventName, typeof(TH).GetGenericTypeName());

        _subsManager.AddSubscription<T, TH>(); // add dicionario handlers
        
        StartBasicConsume();
        
        var channel = _persistentConnection.CreateModel();
        
        var dlqConsumer = new AsyncEventingBasicConsumer(channel);
        dlqConsumer.Received += async (model, ea) =>
        {
            var ack = await ProccessEventError(eventName, ea);
            
            if(ack)
                channel.BasicAck(ea.DeliveryTag, multiple: false);
        };

        channel.BasicConsume(queue: $"{_queueName}_dlq",
            autoAck: false,
            consumer: dlqConsumer);
    }

    private void DoInternalSubscription(string eventName)
    {
        var containsKey = _subsManager.HasSubscriptionsForEvent(eventName);
        if (!containsKey)
        {
            _logger.LogTrace("No subscription found for event {EventName}", eventName);
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            _logger.LogTrace("Binding event {EventName} to queue {QueueName}", eventName, _queueName);
            using (var channel = _persistentConnection.CreateModel())
            {
                channel.QueueBind(queue: _queueName,
                    exchange: _brokerName,
                    routingKey: eventName);
            }
        }
    }

    public void Unsubscribe<T, TH>()
        where T : IntegrationEvent
        where TH : IIntegrationEventHandler<T>
    {
        var eventName = _subsManager.GetEventKey<T>();

        _logger.LogInformation("Unsubscribing from event {EventName}", eventName);

        _subsManager.RemoveSubscription<T, TH>();
    }

    public void Dispose()
    {
        if (_consumerChannel != null)
        {
            _consumerChannel.Dispose();
        }

        _subsManager.Clear();
    }

    private void StartBasicConsume()
    {
        _logger.LogTrace("Starting RabbitMQ basic consume on queue {QueueName}", _queueName);

        if (_consumerChannel != null)
        {
            var consumer = new AsyncEventingBasicConsumer(_consumerChannel);

            consumer.Received += Consumer_Received;
            
            _consumerChannel.BasicConsume(
                queue: _queueName,
                autoAck: false,
                consumer: consumer);
        }
        else
        {
            _logger.LogError("StartBasicConsume can't call on _consumerChannel == null. Queue {QueueName}", _queueName);
        }
    }
    
    private async Task Consumer_Received(object sender, BasicDeliverEventArgs eventArgs)
    {
        var eventName = eventArgs.RoutingKey;
        var message = Encoding.UTF8.GetString(eventArgs.Body.Span);

        try
        {
            if (message.ToLowerInvariant().Contains("throw-fake-exception"))
            {
                throw new InvalidOperationException($"Fake exception requested: \"{message}\"");
            }

            await ProcessEvent(eventName, message);
                
            _consumerChannel.BasicAck(eventArgs.DeliveryTag, multiple: false);
        }
        catch (Exception ex)
        {
            await HandleProcessEventError(eventArgs);
        }
    }

    private async Task HandleProcessEventError(BasicDeliverEventArgs eventArgs)
    {
        if (_hasRetry && PublishDelay(eventArgs))
        {
            _consumerChannel.BasicAck(eventArgs.DeliveryTag, false);
            return;
        }
        
        _consumerChannel.BasicNack(eventArgs.DeliveryTag, false, true);
    }
        
    private bool PublishDelay(BasicDeliverEventArgs eventArgs)
    {
        try
        {
            var queueName = _queueName + ".delay";
            _logger.LogInformation("{ActualDate} :: Publish delay - QueueName: {QueueName}", DateTime.Now, queueName);

            var properties = _consumerChannel.CreateBasicProperties();
            var attemptsValue = GetAttemptsValue(properties);
            
            if (eventArgs.BasicProperties.Headers != null && eventArgs.BasicProperties.Headers.ContainsKey("attempts"))
            {
                attemptsValue = (int)eventArgs.BasicProperties.Headers.First(x => x.Key == "attempts").Value;
                attemptsValue++;
            }
            
            properties.Headers ??= new Dictionary<string, object>();
            properties.Headers.Add(new KeyValuePair<string, object>("attempts", attemptsValue));
            properties.Persistent = true;
            properties.Expiration = GetExpiration(attemptsValue);
            properties.DeliveryMode = 2;

            _consumerChannel.BasicPublish(
                exchange: _brokerName,
                routingKey: queueName,
                mandatory: true,
                basicProperties: properties,
                eventArgs.Body);
                
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "----- ERROR publishing delay message \"{Message}\"", eventArgs.Body);
            return false;
        }
    }

    private static int GetAttemptsValue(IBasicProperties properties)
    {
        var attemptsValue = 0;

        if (properties.Headers != null && properties.Headers.Keys.Contains("attempts"))
        {
            var attempHeader = properties.Headers.First(x => x.Key == "attempts");
            properties.Headers.Remove(attempHeader);
            attemptsValue = (int) attempHeader.Value;
            attemptsValue++;
        }

        return attemptsValue;
    }

    private string GetExpiration(int attempts)
    {
        var expiration = attempts switch
        {
            < 5 => 60000,
            < 10 => 300000,
            < 20 => 3600000,
            _ => 86400000
        };

        return expiration.ToString();
    }

    private IModel CreateConsumerChannel()
    {
        if (!_persistentConnection.IsConnected)
        {
            _persistentConnection.TryConnect();
        }

        _logger.LogTrace("Creating RabbitMQ consumer channel");

        var channel = _persistentConnection.CreateModel();

        _logger.LogTrace("Declared exchange {BrokerName}", _brokerName);
        channel.ExchangeDeclare(exchange: _brokerName,
            type: "direct",
            durable: true);

        _logger.LogTrace("Declared dlq queue {QueueName}", $"{_queueName}_dlq");
        channel.QueueDeclare(queue: $"{_queueName}_dlq",
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

        _logger.LogTrace("Declared queue {QueueName}", _queueName);
        channel.QueueDeclare(queue: _queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object>
            {
                { "x-dead-letter-exchange", "" },
                { "x-dead-letter-routing-key", $"{_queueName}_dlq" },
                { "x-message-ttl", 60000 }
            });

        channel.CallbackException += (sender, ea) =>
        {
            _logger.LogWarning(ea.Exception, 
                "Recreating RabbitMQ consumer channel on broker {BrokerName} and queue {QueueName}", _brokerName, _queueName);

            _consumerChannel.Dispose();
            _consumerChannel = CreateConsumerChannel();
            StartBasicConsume();
        };

        return channel;
    }

    private async Task ProcessEvent(string eventName, string message)
    {
        _logger.LogTrace("Processing RabbitMQ event: {EventName}", eventName);

        if (_subsManager.HasSubscriptionsForEvent(eventName))
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var subscriptions = _subsManager.GetHandlersForEvent(eventName);
                foreach (var subscription in subscriptions)
                {
                    var handler = scope.ServiceProvider.GetService(subscription.HandlerType);
                        
                    _logger.LogTrace("Subscription typed handler: {Handler}", handler);
                    if (handler == null)
                    {
                        _logger.LogWarning("Could not resolve typed handler: {HandlerType}", subscription.HandlerType);
                        continue;
                    }
                        
                    var eventType = _subsManager.GetEventTypeByName(eventName);
                    
                    var integrationEvent = JsonConvert.DeserializeObject(message, eventType, 
                        new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
                    
                    var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);

                    await Task.Yield();
                    await (Task)concreteType.GetMethod("Handle").Invoke(handler, new object[] { integrationEvent });
                }
            }
        }
        else
        {
            _logger.LogWarning("No subscription for RabbitMQ event: {EventName}", eventName);
        }
    }

    public async Task<bool> ProccessEventError(string eventName, BasicDeliverEventArgs @event)
    {
        _logger.LogTrace("Processing RabbitMQ error method: {EventName}", eventName);
        
        var message = Encoding.UTF8.GetString(@event.Body.Span);

        if (_subsManager.HasSubscriptionsForEvent(eventName))
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var subscriptions = _subsManager.GetHandlersForEvent(eventName);
                foreach (var subscription in subscriptions)
                {
                    var handler = scope.ServiceProvider.GetService(subscription.HandlerType);
                        
                    _logger.LogTrace("Subscription typed error handler: {Handler}", handler);
                    if (handler == null)
                    {
                        _logger.LogWarning("Could not resolve typed error handler: {HandlerType}", subscription.HandlerType);
                        continue;
                    }
                    
                    var eventType = _subsManager.GetEventTypeByName(eventName);
                    
                    var integrationEvent = JsonConvert.DeserializeObject(message, eventType, 
                        new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
                    
                    var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);

                    await Task.Yield();
                    return await (Task<bool>)concreteType.GetMethod("HandleError").Invoke(handler, new object[] { integrationEvent });
                }
            }
        }
        else
        {
            _logger.LogWarning("No subscription for RabbitMQ event: {EventName}", eventName);
        }

        return false;
    }
}