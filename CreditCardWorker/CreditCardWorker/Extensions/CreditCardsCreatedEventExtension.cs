using Messenger.EventBus.EventBus.Events;

namespace CreditCardWorker.Extensions;

public static class CreditCardsCreatedEventExtension
{
    public static string Log(this CreditCardsCreatedEvent creditCardsCreatedEvent)
    {
        var creditCardToString =  creditCardsCreatedEvent.CreditProposalMessage.Select(creditCard => 
            $"Cartão final: {creditCard.LastCardNumbers}"
            +$"\nLimite: {creditCard.CreditLimit:C}"
            +$"\nTítular: {creditCard.CardHolderName}"
            +$"\nData de Vencimento: {creditCard.ExpiryDate.ToShortDateString()}");
        
        return string.Join("\n\n", creditCardToString);
    }
}