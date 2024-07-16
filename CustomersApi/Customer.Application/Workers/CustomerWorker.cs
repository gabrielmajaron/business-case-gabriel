using Customer.Application.Handlers;
using Messenger.EventBus.EventBus.Abstractions;
using Messenger.EventBus.EventBus.Events;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Customer.Application.Workers;

public class CustomerWorker : BackgroundService
{
    private readonly ILogger<CustomerWorker> _logger;
    private readonly IEventBus _eventBus;
    
    public CustomerWorker(ILogger<CustomerWorker> logger, IEventBus eventBus)
    {
        _logger = logger;
        _eventBus = eventBus;
        
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(eventBus);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
        
        await Task.Run(() =>
        {
            _eventBus.Subscribe<CreditProposalGenerationErrorEvent, CreditEventsHandler>();
            _eventBus.Subscribe<CreditCardGenerationErrorEvent, CreditEventsHandler>();
        });
    }
}