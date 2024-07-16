using Messenger.EventBus.EventBus.Abstractions;
using Messenger.EventBus.EventBus.Events;
using Microsoft.Extensions.Logging;

namespace Customer.Application.Handlers;

public class CreditEventsHandler : IIntegrationEventHandler<CreditProposalGenerationErrorEvent>, IIntegrationEventHandler<CreditCardGenerationErrorEvent>
{
    private readonly ILogger<CreditEventsHandler> _logger;
    
    public CreditEventsHandler(ILogger<CreditEventsHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    public async Task Handle(CreditProposalGenerationErrorEvent @event)
    {
        _logger.LogInformation($"Erro ao criar proposta para o User: {@event.UserCreatedMessage?.Id}. EventId: {@event.Id}");
    }

    public async Task<bool> HandleError(CreditProposalGenerationErrorEvent @event)
    {
        _logger.LogError($"Erro ao processar evento: {@event.Id}");
        return false;
    }

    public async Task Handle(CreditCardGenerationErrorEvent @event)
    {
        _logger.LogInformation($"Erro ao criar cartões de crédito para o User: {@event.CreditProposalMessage?.UserCreatedMessage?.Id}. EventId: {@event.Id}");
    }

    public async Task<bool> HandleError(CreditCardGenerationErrorEvent @event)
    {
        _logger.LogError($"Erro ao processar evento: {@event.Id}");
        return false;
    }
}