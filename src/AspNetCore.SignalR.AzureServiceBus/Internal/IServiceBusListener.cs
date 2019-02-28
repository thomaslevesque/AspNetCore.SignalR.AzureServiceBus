using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;

namespace AspNetCore.SignalR.AzureServiceBus.Internal
{
    internal interface IServiceBusListener
    {
        Task StartListeningAsync(Func<Message, CancellationToken, Task> onMessageReceived, Func<ExceptionReceivedEventArgs, Task> onError, CancellationToken cancellationToken);
        Task StopListeningAsync(CancellationToken cancellationToken);
    }
}