using Messenger.Configuration;

namespace Messenger.EventBus.EventBusRabbitMQ.Configuration
{
    public class EventBusRabbitMQBaseSettings : RabbitMQBaseSettings
    {
        public string RABBITMQ_BROKER { get; set; }
        public string RABBITMQ_QUEUE { get; set; }
        public string RABBITMQ_CONN_RETRIES { get; set; }
        public bool RABBITMQ_HAS_DELAY_RETRY { get; set; }
    }
}