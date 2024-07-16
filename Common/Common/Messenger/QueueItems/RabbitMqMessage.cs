namespace Messenger.QueueItems
{
    public class RabbitMqMessage
    {
        public string Id { get; set; }
        public string Publication { get; set; }
        public string FirstOrigin { get; set; }
        public string LastOrigin { get; set; }
        public int Attempts { get; set; }
        public string Content { get; set; }
    }
}