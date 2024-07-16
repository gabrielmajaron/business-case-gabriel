using System.Collections.Generic;
using Messenger.Enums;
using Messenger.EventBus.EventBus.EventContents;

namespace Messenger.EventBus.EventBus.Events;

public class CreditCardsCreatedEvent : IntegrationEvent
{
    public List<CreditCardMessage> CreditCardsMessage { get; set; }
    public override EventNameEnum EventName => EventNameEnum.CreditCardsCreated;
}