using Customer.Core.DataTransferObjects;
using Customer.Core.Interfaces.Handlers;
using Messenger.EventBus.EventBus.EventContents;
using Messenger.EventBus.EventBus.Events;
using Messenger.Extensions;
using Microsoft.Extensions.Logging;

namespace Customer.Application.Handlers;

public class CustomerHandler : ICustomerHandler
{
    private readonly IEventHandler _eventHandler;
    private readonly ILogger<CustomerHandler> _logger;
    
    public CustomerHandler(IEventHandler eventHandler, ILogger<CustomerHandler> logger)
    {
        _eventHandler = eventHandler;
        _logger = logger;

        ArgumentNullException.ThrowIfNull(eventHandler);
        ArgumentNullException.ThrowIfNull(logger);
    }
    
    // Algumas coisas não foram implementadas (comentadas) devido ao foco principal deste exemplo ser a mensageria
    public async Task CreateAsync(CreateCustomerRequest createCustomerRequest)
    {
        // Validação aqui
        // Inserção de novo cliente no banco de dados aqui
        var @event = GetCustomerCreatedEvent(createCustomerRequest);
        await _eventHandler.PublishEvent(@event);
        _logger.LogInformation($"Novo usuário criado. UserId: {@event.UserCreatedMessage.Id}. Evento {@event.EventName.GetDescription()} publicado.");
    }

    private CustomerCreatedEvent GetCustomerCreatedEvent(CreateCustomerRequest createCustomerRequest)
    {
        return new CustomerCreatedEvent
        {
            
            UserCreatedMessage = new UserCreatedMessage
            {
                // Classe de mapping aqui
                Id = Guid.NewGuid(),
                Name = createCustomerRequest.UserPersonalData.Name,
                SecondName = createCustomerRequest.UserPersonalData.SecondName,
                MonthlyIncome = createCustomerRequest.UserBusinessInformation.MonthlyIncome,
                CreditCardsQuantity = createCustomerRequest.CreditCardsQuantity
            }
        };
    }
}