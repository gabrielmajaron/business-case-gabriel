using Messenger.Enums;
using Messenger.EventBus.EventBus.EventContents;

namespace Messenger.EventBus.EventBus.Events;

public class CustomerCreatedEvent : IntegrationEvent
{
    public UserCreatedMessage UserCreatedMessage { get; set; }
    public override EventNameEnum EventName => EventNameEnum.CustomerCreated;
}