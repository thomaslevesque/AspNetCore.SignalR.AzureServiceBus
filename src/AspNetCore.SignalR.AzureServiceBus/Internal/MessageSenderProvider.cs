using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.SignalR.AzureServiceBus.Internal
{
    internal class MessageSenderProvider : IMessageSenderProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ConcurrentDictionary<string, IMessageSender> _senders;

        public MessageSenderProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _senders = new ConcurrentDictionary<string, IMessageSender>();
        }

        public IMessageSender GetMessageSenderForHub(string hubTypeName)
        {
            return _senders.GetOrAdd(
                hubTypeName,
                name =>
                {
                    var hubType = Type.GetType(name);
                    if (hubType == null)
                    {
                        return null;
                    }

                    var senderType = typeof(MessageSenderHubLifetimeManager<>).MakeGenericType(hubType);
                    return (IMessageSender) _serviceProvider.GetRequiredService(senderType);
                });
        }
    }
}
