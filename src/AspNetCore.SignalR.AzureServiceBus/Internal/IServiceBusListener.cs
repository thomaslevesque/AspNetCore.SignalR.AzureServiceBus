using System;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.SignalR.AzureServiceBus.Internal
{
    internal interface IServiceBusListener
    {
        Task StartListeningAsync(Func<SignalRMessage, CancellationToken, Task> onMessageReceived, CancellationToken cancellationToken);
        Task StopListeningAsync(CancellationToken cancellationToken);
    }
}
