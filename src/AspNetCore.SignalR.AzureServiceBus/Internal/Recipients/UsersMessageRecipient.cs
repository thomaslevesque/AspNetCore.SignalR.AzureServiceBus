using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.SignalR.AzureServiceBus.Internal.Recipients
{
    internal class UsersMessageRecipient : SignalRMessageRecipient
    {
        public IReadOnlyList<string> UserIds { get; set; }

        public override Task SendCoreAsync(IMessageSender sender, string method, object[] args, CancellationToken cancellationToken)
        {
            return sender.SendUsersAsync(UserIds, method, args, cancellationToken);
        }
    }
}
