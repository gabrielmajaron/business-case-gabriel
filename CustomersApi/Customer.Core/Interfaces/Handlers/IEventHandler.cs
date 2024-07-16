using Messenger.EventBus.EventBus.Events;

namespace Customer.Core.Interfaces.Handlers;

public interface IEventHandler
{
    Task PublishEvent(IntegrationEvent integrationEvent);
}