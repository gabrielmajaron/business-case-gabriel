using Messenger.Enums;
using Messenger.EventBus.EventBus.EventContents;

namespace Messenger.EventBus.EventBus.Events;

public class CreditCardGenerationErrorEvent : IntegrationEvent
{
    public CreditProposalMessage CreditProposalMessage { get; set; }
    public override EventNameEnum EventName => EventNameEnum.CreditCardGenerationError;
}