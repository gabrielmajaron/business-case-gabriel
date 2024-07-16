using Customer.Core.Interfaces.Handlers;
using Messenger.EventBus.EventBus.Abstractions;
using Messenger.EventBus.EventBus.Events;
using Messenger.Extensions;
using Microsoft.Extensions.Logging;

namespace Customer.Application.Handlers;

public class EventHandler : IEventHandler
{
    private readonly IEventBus _eventBus;
    private readonly ILogger<EventHandler> _logger;
    
    public EventHandler(IEventBus eventBus, ILogger<EventHandler> logger)
    {
        _eventBus = eventBus;
        _logger = logger;

        ArgumentNullException.ThrowIfNull(nameof(eventBus));
        ArgumentNullException.ThrowIfNull(nameof(logger));
    }

    public async Task PublishEvent(IntegrationEvent integrationEvent)
    {
        await Task.Run(() =>
        {
            try
            {
                _eventBus.Publish(integrationEvent);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to publish event {eventName}", integrationEvent.EventName.GetDescription()); 
            }
        });
    }
}