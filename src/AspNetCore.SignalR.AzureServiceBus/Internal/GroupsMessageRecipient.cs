using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace AspNetCore.SignalR.AzureServiceBus.Internal
{
    internal class GroupsMessageRecipient : SignalRMessageRecipient
    {
        public IReadOnlyList<string> GroupNames { get; set; }

        public override Task SendCoreAsync(IMessageSender sender, string method, object[] args, CancellationToken cancellationToken)
        {
            return sender.SendGroupsAsync(GroupNames, method, args, cancellationToken);
        }
    }
}
