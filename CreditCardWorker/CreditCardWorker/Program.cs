using CreditCardWorker.Handlers;
using Messenger.EventBus.EventBusRabbitMQ.Configuration;

namespace CreditCardWorker;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddEventBusRabbitMQ(builder.Configuration);
        builder.Services.AddHostedService<Worker>();
        builder.Services.AddSingleton<CreditCardHandler>();

        var host = builder.Build();
        host.Run();
    }
}