using Messenger.Enums;
using Messenger.EventBus.EventBus.EventContents;

namespace Messenger.EventBus.EventBus.Events;

public class CreditProposalGenerationErrorEvent : IntegrationEvent
{
    public UserCreatedMessage UserCreatedMessage { get; set; }
    public override EventNameEnum EventName => EventNameEnum.CreditProposalGenerationError;
}