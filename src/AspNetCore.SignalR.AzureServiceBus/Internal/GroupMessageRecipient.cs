using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace AspNetCore.SignalR.AzureServiceBus.Internal
{
    internal class GroupMessageRecipient : SignalRMessageRecipient
    {
        public string GroupName { get; set; }

        public IReadOnlyList<string> ExcludedConnectionIds { get; set; }

        public override Task SendCoreAsync(IMessageSender sender, string method, object[] args, CancellationToken cancellationToken)
        {
            return sender.SendGroupExceptAsync(GroupName, method, args, ExcludedConnectionIds ?? Array.Empty<string>(), cancellationToken);
        }
    }
}
