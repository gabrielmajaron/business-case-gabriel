using Messenger.QueueItems;

namespace Messenger.Extensions
{
    public static class RabbitMqMessageExtension
    {
        public static string GetMsgLogError(this RabbitMqMessage processWorkerCommand) => 
            "{1} :: Error in queue consume - QueueName: {2} - ErrorMessage: {3}";
        
        public static string GetMsgPublishDelay(this RabbitMqMessage processWorkerCommand) => 
            "{1} :: Publish delay - QueueName: {2}";
        
        public static string GetMsgLogErrorPublishDelay(this RabbitMqMessage processWorkerCommand) => 
            "{1} :: Error in publish delay - ErrorMessage: {2}";

    }
}