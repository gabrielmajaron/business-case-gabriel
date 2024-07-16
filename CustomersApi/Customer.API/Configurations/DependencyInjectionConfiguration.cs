using Customer.Application.Handlers;
using Customer.Core.Interfaces.Handlers;
using EventHandler = Customer.Application.Handlers.EventHandler;

namespace ParanaBanco.CustomerAPI.Configurations;

public static class DependencyInjectionConfiguration
{
    public static void AddDependencyInjectionConfiguration(this IServiceCollection services)
    {
        services.AddSingleton<IEventHandler, EventHandler>();
        services.AddSingleton<ICustomerHandler, CustomerHandler>();
        services.AddSingleton<CreditEventsHandler>();
    }
}