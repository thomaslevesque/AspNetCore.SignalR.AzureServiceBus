using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.SignalR.AzureServiceBus.Internal.Recipients
{
    internal class ClientMessageRecipient : SignalRMessageRecipient
    {
        public string ConnectionId { get; set; }

        public override Task SendCoreAsync(IMessageSender sender, string method, object[] args, CancellationToken cancellationToken)
        {
            return sender.SendConnectionAsync(ConnectionId, method, args, cancellationToken);
        }
    }
}
