namespace Messenger.EventBus.EventBus.EventContents;

public class CreditProposalMessage
{
    public UserCreatedMessage UserCreatedMessage { get; set; }
    public decimal MaxCreditLimit { get; set; }
}