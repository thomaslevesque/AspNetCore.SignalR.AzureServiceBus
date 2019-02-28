using System.Threading.Tasks;

namespace AspNetCore.SignalR.AzureServiceBus.Internal
{
    internal interface IServiceBusPublisher
    {
        Task PublishAsync(SignalRMessage message);
    }
}