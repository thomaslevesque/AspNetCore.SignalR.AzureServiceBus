using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace AspNetCore.SignalR.AzureServiceBus.Internal
{
    internal class ClientsMessageRecipient : SignalRMessageRecipient
    {
        public IReadOnlyList<string> ConnectionIds { get; set; }

        public override Task SendCoreAsync(IMessageSender sender, string method, object[] args, CancellationToken cancellationToken)
        {
            return sender.SendConnectionsAsync(ConnectionIds, method, args, cancellationToken);
        }
    }
}
