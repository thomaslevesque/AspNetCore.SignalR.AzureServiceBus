using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.SignalR.AzureServiceBus.Internal.Recipients
{
    internal class UserMessageRecipient : SignalRMessageRecipient
    {
        public string UserId { get; set; }

        public override Task SendCoreAsync(IMessageSender sender, string method, object[] args, CancellationToken cancellationToken)
        {
            return sender.SendUserAsync(UserId, method, args, cancellationToken);
        }
    }
}
