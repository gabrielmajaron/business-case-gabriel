using System.Threading.Tasks;

namespace Messenger.EventBus.EventBus.Abstractions
{
    public interface IDynamicIntegrationEventHandler
    {
        Task Handle(dynamic eventData);
    }
}