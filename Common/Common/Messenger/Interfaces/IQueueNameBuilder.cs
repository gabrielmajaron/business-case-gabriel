namespace Messenger.Interfaces
{
    public interface IQueueNameBuilder
    {
        string BuildCompanyQueueName(string queueName);
        string BuildCompanyDelayQueueName(string queueName);
        string GetExchangeDefault();
    }
}