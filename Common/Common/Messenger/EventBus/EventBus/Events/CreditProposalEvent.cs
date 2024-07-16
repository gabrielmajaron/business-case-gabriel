using Messenger.Enums;
using Messenger.EventBus.EventBus.EventContents;

namespace Messenger.EventBus.EventBus.Events;

public class CreditProposalEvent : IntegrationEvent
{
    public CreditProposalMessage CreditProposalMessage { get; set; }
    public override EventNameEnum EventName => EventNameEnum.CreditProposalCreated;
}