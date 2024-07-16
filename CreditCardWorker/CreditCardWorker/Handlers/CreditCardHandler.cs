using Bogus;
using CreditCardWorker.DataTransferObjects;
using CreditCardWorker.Extensions;
using Messenger.EventBus.EventBus.Abstractions;
using Messenger.EventBus.EventBus.EventContents;
using Messenger.EventBus.EventBus.Events;

namespace CreditCardWorker.Handlers;

public class CreditCardHandler : IIntegrationEventHandler<CreditProposalEvent>
{
    private readonly IEventBus _eventBus;
    private readonly ILogger<CreditCardHandler> _logger;
    
    public CreditCardHandler(IEventBus eventBus, ILogger<CreditCardHandler> logger)
    {
        _eventBus = eventBus;
        _logger = logger;

        ArgumentNullException.ThrowIfNull(eventBus);
        ArgumentNullException.ThrowIfNull(logger);
    }
    

    public async Task Handle(CreditProposalEvent @event)
    {
        try
        {
            //throw new Exception($"Problema ao criar cartões de crédito para o usuário: {@event.CreditProposalMessage.UserCreatedMessage.Id}");
            
            var creditProposal = @event.CreditProposalMessage;
            var creditCardsQuantity = creditProposal.UserCreatedMessage.CreditCardsQuantity;
            
            var creditCards = CreateCreditCards(creditCardsQuantity, creditProposal);
            var creditCardsCreatedEvent = GetCreditCardsEvent(creditCards);
            
            // Registro no banco de dados aqui
            _eventBus.Publish(creditCardsCreatedEvent);
            
            _logger.LogInformation($"Cartões de créditos criados para o usuário: {creditProposal.UserCreatedMessage.Id}"
                +$"\n{creditCardsCreatedEvent.Log()}");
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            throw;
        }
    }
    
    /// <summary>
    /// Método que é disparado quando ocorre um erro no processamento do item da fila.
    /// </summary>
    /// <param name="event"></param>
    /// <returns> Retorna um booleano que remove da DLQ caso true. </returns>
    public async Task<bool> HandleError(CreditProposalEvent @event)
    {
        try
        {
            _logger.LogInformation("Enviando evento creditcardgeneration.error");

            var errorEvent = new CreditCardGenerationErrorEvent
            {
                CreditProposalMessage = @event.CreditProposalMessage
            };

            _eventBus.Publish(errorEvent);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return false;
        }
        return false;
    }

    private List<CreditCard> CreateCreditCards(int creditCardsQuantity, CreditProposalMessage creditProposal)
    {
        var creditCards = new List<CreditCard>();
        var faker = new Faker();
        var expiryDate = faker.Date.Future();
        var cardHolderName = $"{creditProposal.UserCreatedMessage.Name} {creditProposal.UserCreatedMessage.SecondName}";

        for (var i = 0; i < creditCardsQuantity; i++)
        {
            creditCards.Add(new CreditCard {
                CreditLimit = creditProposal.MaxCreditLimit,
                ExpiryDate = expiryDate,
                CardHolderName = cardHolderName,
                CardNumber = faker.Finance.CreditCardNumber()
            });
        }

        return creditCards;
    }

    private CreditCardsCreatedEvent GetCreditCardsEvent(List<CreditCard> creditCards) =>
        new ()
        {
            CreditProposalMessage = creditCards.Select(c => new CreditCardMessage {
            CreditLimit = c.CreditLimit,
            ExpiryDate = c.ExpiryDate,
            CardHolderName = c.CardHolderName,
            LastCardNumbers = c.CardNumber.Substring(c.CardNumber.Length-4)
        }).ToList()
    };
}