using Messenger.Interfaces;

namespace Messenger.Builders
{
    public class QueueNameBuilder : IQueueNameBuilder
    {
        private const string COMPANY_QUEUE_NAME_PREFIX = "parana_banco";
        private const string DELAY_QUEUE_NAME_IDENTIFIER = "delay";
        
        public string BuildCompanyQueueName(string queueName)
            => $"{COMPANY_QUEUE_NAME_PREFIX}.{queueName}";
        
        public string BuildCompanyDelayQueueName(string queueName)
            => $"{COMPANY_QUEUE_NAME_PREFIX}.{queueName}.{DELAY_QUEUE_NAME_IDENTIFIER}";

        public string GetExchangeDefault()
            => COMPANY_QUEUE_NAME_PREFIX;
    }
}