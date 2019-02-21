using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace AspNetCore.SignalR.AzureServiceBus.Internal
{
    internal class UserMessageRecipients : SignalRMessageRecipient
    {
        public string UserId { get; set; }

        public override Task SendCoreAsync(IMessageSender sender, string method, object[] args, CancellationToken cancellationToken)
        {
            return sender.SendUserAsync(UserId, method, args, cancellationToken);
        }
    }
}
