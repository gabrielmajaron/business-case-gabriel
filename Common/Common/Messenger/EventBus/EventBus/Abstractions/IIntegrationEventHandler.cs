using System.Threading.Tasks;
using Messenger.EventBus.EventBus.Events;

namespace Messenger.EventBus.EventBus.Abstractions
{
    public interface IIntegrationEventHandler<in TIntegrationEvent> : IIntegrationEventHandler
        where TIntegrationEvent : IntegrationEvent
    {
        /// <summary>
        /// Método executado quando um item da fila é consumido.
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        Task Handle(TIntegrationEvent @event);
        /// <summary>
        /// Método que é disparado quando ocorre um erro no processamento do item da fila.
        /// </summary>
        /// <param name="event"></param>
        /// <returns> Retorna um booleano que remove da DLQ caso seja true. </returns>
        Task<bool> HandleError(TIntegrationEvent @event);
    }

    public interface IIntegrationEventHandler
    {
    }
}