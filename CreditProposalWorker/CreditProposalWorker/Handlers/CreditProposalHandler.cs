using CreditProposalWorker.Extensions;
using CreditProposalWorker.Interfaces;
using Messenger.EventBus.EventBus.Abstractions;
using Messenger.EventBus.EventBus.EventContents;
using Messenger.EventBus.EventBus.Events;

namespace CreditProposalWorker.Handlers;

public class CreditProposalHandler : IIntegrationEventHandler<CustomerCreatedEvent>
{
    private readonly ICreditProposalCalculator _creditProposalCalculator;
    private readonly IEventBus _eventBus;
    private readonly ILogger<CreditProposalHandler> _logger;
    
    public CreditProposalHandler(ICreditProposalCalculator creditProposalCalculator, IEventBus eventBus, ILogger<CreditProposalHandler> logger)
    {
        _creditProposalCalculator = creditProposalCalculator;
        _eventBus = eventBus;
        _logger = logger;

        ArgumentNullException.ThrowIfNull(creditProposalCalculator);
        ArgumentNullException.ThrowIfNull(eventBus);
        ArgumentNullException.ThrowIfNull(logger);
    }
    
    public async Task Handle(CustomerCreatedEvent @event)
    {
        try
        {
            //throw new Exception($"Problema ao criar proposta de crédito para o usuário: {@event.UserCreatedMessage.Id}");
            
            var userMsgContent = @event.UserCreatedMessage;

            var creditProposalEvent = new CreditProposalEvent
            {
                CreditProposalMessage = new CreditProposalMessage
                {
                    UserCreatedMessage = userMsgContent,
                    MaxCreditLimit = _creditProposalCalculator.GetMaxCreditLimit(userMsgContent.MonthlyIncome)
                }
            };

            // Registro no banco de dados aqui
            _eventBus.Publish(creditProposalEvent);
            _logger.LogInformation($"Proposta de crédito criada:"
                +$"\n{creditProposalEvent.Log()}");
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
    public async Task<bool> HandleError(CustomerCreatedEvent @event)
    {
        try
        {
            _logger.LogInformation("Enviando evento creditproposalgeneration.error");

            var errorEvent = new CreditProposalGenerationErrorEvent
            {
                UserCreatedMessage = @event.UserCreatedMessage
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
}
