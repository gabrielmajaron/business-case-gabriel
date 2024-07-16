using System;
using Messenger.Enums;

namespace Messenger.EventBus.EventBus.Events
{
    public abstract class IntegrationEvent
    {
        public IntegrationEvent()
        {
            Id = Guid.NewGuid();
            CreationDate = DateTime.UtcNow;
        }
        public Guid Id { get; private set; }
        public DateTime CreationDate { get; private set; }
        public abstract EventNameEnum EventName { get; }
    }
}