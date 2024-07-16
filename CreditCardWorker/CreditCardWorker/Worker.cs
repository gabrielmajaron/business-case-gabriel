using CreditCardWorker.Handlers;
using Messenger.EventBus.EventBus.Abstractions;
using Messenger.EventBus.EventBus.Events;

namespace CreditCardWorker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IEventBus _eventBus;

    public Worker(ILogger<Worker> logger, IEventBus eventBus)
    {
        _logger = logger;
        _eventBus = eventBus;
        
        ArgumentNullException.ThrowIfNull(eventBus);
        ArgumentNullException.ThrowIfNull(logger);
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
        
        await Task.Run(() =>
        {
            _eventBus.Subscribe<CreditProposalEvent, CreditCardHandler>();
        });
    }
}