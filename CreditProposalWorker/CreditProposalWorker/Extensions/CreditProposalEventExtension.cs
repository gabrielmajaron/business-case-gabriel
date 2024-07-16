using Messenger.EventBus.EventBus.Events;

namespace CreditProposalWorker.Extensions;

public static class CreditProposalEventExtension
{
    public static string Log(this CreditProposalEvent creditProposalEvent)
    {
        var user = creditProposalEvent.CreditProposalMessage.UserCreatedMessage;
        return $"Id do Usuário: {user.Id}"
               +$"\nNome: {user.Name} {user.SecondName}" 
               +$"\nLimite de crédito: {creditProposalEvent.CreditProposalMessage.MaxCreditLimit:C}";
    }
}