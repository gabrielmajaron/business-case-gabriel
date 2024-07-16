using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace Messenger.Interfaces
{
    public interface IRBMQConnection
    {
        void Publish(string queueName, dynamic data);
        void PublishList(string queueName, List<object> list);
        void Consume(string queueName, Func<string, Task<int>> execute);
        IModel GetChannel(string name, string queueName);
    }
}