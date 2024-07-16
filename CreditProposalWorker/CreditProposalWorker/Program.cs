using CreditProposalWorker.Handlers;
using CreditProposalWorker.Interfaces;
using Messenger.EventBus.EventBusRabbitMQ.Configuration;

namespace CreditProposalWorker;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddHostedService<Worker>();
        builder.Services.AddEventBusRabbitMQ(builder.Configuration);
        builder.Services.AddSingleton<CreditProposalHandler>();
        builder.Services.AddSingleton<ICreditProposalCalculator, CreditProposalCalculator>();

        var host = builder.Build();
        host.Run();
    }
}