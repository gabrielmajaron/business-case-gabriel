using CreditProposalWorker.Handlers;
using Messenger.EventBus.EventBus.Abstractions;
using Messenger.EventBus.EventBus.Events;

namespace CreditProposalWorker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IEventBus _eventBus;

    
    public Worker(ILogger<Worker> logger, IEventBus eventBus)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
        
        await Task.Run(() =>
        {
            _eventBus.Subscribe<CustomerCreatedEvent, CreditProposalHandler>();
        });
    }
}