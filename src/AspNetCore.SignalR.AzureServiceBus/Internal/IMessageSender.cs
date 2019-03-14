using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.SignalR.AzureServiceBus.Internal
{
    internal interface IMessageSender
    {
        Task SendAllAsync(string methodName, object[] args, CancellationToken cancellationToken = default);
        Task SendAllExceptAsync(string methodName, object[] args, IReadOnlyList<string> excludedConnectionIds, CancellationToken cancellationToken = default);
        Task SendConnectionAsync(string connectionId, string methodName, object[] args, CancellationToken cancellationToken = default);
        Task SendConnectionsAsync(IReadOnlyList<string> connectionIds, string methodName, object[] args, CancellationToken cancellationToken = default);
        Task SendGroupAsync(string groupName, string methodName, object[] args, CancellationToken cancellationToken = default);
        Task SendGroupsAsync(IReadOnlyList<string> groupNames, string methodName, object[] args, CancellationToken cancellationToken = default);
        Task SendGroupExceptAsync(string groupName, string methodName, object[] args, IReadOnlyList<string> excludedConnectionIds, CancellationToken cancellationToken = default);
        Task SendUserAsync(string userId, string methodName, object[] args, CancellationToken cancellationToken = default);
        Task SendUsersAsync(IReadOnlyList<string> userIds, string methodName, object[] args, CancellationToken cancellationToken = default);
    }
}
