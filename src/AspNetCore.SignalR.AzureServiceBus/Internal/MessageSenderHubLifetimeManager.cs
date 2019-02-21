using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace AspNetCore.SignalR.AzureServiceBus.Internal
{
    internal class MessageSenderHubLifetimeManager<THub> : DefaultHubLifetimeManager<THub>, IMessageSender
        where THub : Hub
    {
        public MessageSenderHubLifetimeManager(ILogger<MessageSenderHubLifetimeManager<THub>> logger) : base(logger)
        {
        }
    }
}
