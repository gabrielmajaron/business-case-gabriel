using System;
using RabbitMQ.Client;

namespace Messenger.EventBus.EventBusRabbitMQ
{
    public interface IRabbitMQPersistentConnection
        : IDisposable
    {
        bool IsConnected { get; }
        bool TryConnect();
        IModel CreateModel();
    }
}