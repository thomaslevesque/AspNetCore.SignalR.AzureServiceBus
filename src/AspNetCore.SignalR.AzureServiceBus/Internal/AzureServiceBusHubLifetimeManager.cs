using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace AspNetCore.SignalR.AzureServiceBus.Internal
{
    internal class AzureServiceBusHubLifetimeManager<THub> : HubLifetimeManager<THub>
        where THub : Hub
    {
        private static readonly Guid NodeId = Guid.NewGuid();
        private readonly INodeIdProvider _nodeIdProvider;
        private readonly MessageSenderHubLifetimeManager<THub> _messageSender;
        private readonly TopicClient _topicClient;
        private readonly SignalRAzureServiceBusOptions _options;

        public AzureServiceBusHubLifetimeManager(
            INodeIdProvider nodeIdProvider,
            MessageSenderHubLifetimeManager<THub> messageSender,
            IOptions<SignalRAzureServiceBusOptions> options)
        {
            _nodeIdProvider = nodeIdProvider;
            _messageSender = messageSender;
            _options = options.Value;
            _topicClient = new TopicClient(_options.ConnectionString, _options.TopicName);
        }

        public override Task AddToGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _messageSender.AddToGroupAsync(connectionId, groupName, cancellationToken);
        }

        public override Task OnConnectedAsync(HubConnectionContext connection)
        {
            return _messageSender.OnConnectedAsync(connection);
        }

        public override Task OnDisconnectedAsync(HubConnectionContext connection)
        {
            return _messageSender.OnDisconnectedAsync(connection);
        }

        public override Task RemoveFromGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _messageSender.RemoveFromGroupAsync(connectionId, groupName, cancellationToken);
        }

        public override async Task SendAllAsync(string methodName, object[] args, CancellationToken cancellationToken = default(CancellationToken))
        {
            await _messageSender.SendAllAsync(methodName, args, cancellationToken);
            var message = CreateMessage(methodName, args, new AllMessageRecipient());
            await PublishAsync(message);
        }

        public override async Task SendAllExceptAsync(string methodName, object[] args, IReadOnlyList<string> excludedConnectionIds, CancellationToken cancellationToken = default(CancellationToken))
        {
            await _messageSender.SendAllExceptAsync(methodName, args, excludedConnectionIds, cancellationToken);
            var message = CreateMessage(methodName, args, new AllMessageRecipient { ExcludedConnectionIds = excludedConnectionIds });
            await PublishAsync(message);
        }

        public override async Task SendConnectionAsync(string connectionId, string methodName, object[] args, CancellationToken cancellationToken = default(CancellationToken))
        {
            await _messageSender.SendConnectionAsync(connectionId, methodName, args, cancellationToken);
            var message = CreateMessage(methodName, args, new ClientMessageRecipient { ConnectionId = connectionId });
            await PublishAsync(message);
        }

        public override async Task SendConnectionsAsync(IReadOnlyList<string> connectionIds, string methodName, object[] args, CancellationToken cancellationToken = default(CancellationToken))
        {
            await _messageSender.SendConnectionsAsync(connectionIds, methodName, args, cancellationToken);
            var message = CreateMessage(methodName, args, new ClientsMessageRecipient { ConnectionIds = connectionIds });
            await PublishAsync(message);
        }

        public override async Task SendGroupAsync(string groupName, string methodName, object[] args, CancellationToken cancellationToken = default(CancellationToken))
        {
            await _messageSender.SendGroupAsync(groupName, methodName, args, cancellationToken);
            var message = CreateMessage(methodName, args, new GroupMessageRecipient { GroupName = groupName });
            await PublishAsync(message);
        }

        public override async Task SendGroupExceptAsync(string groupName, string methodName, object[] args, IReadOnlyList<string> excludedConnectionIds, CancellationToken cancellationToken = default(CancellationToken))
        {
            await _messageSender.SendGroupExceptAsync(groupName, methodName, args, excludedConnectionIds, cancellationToken);
            var message = CreateMessage(methodName, args, new GroupMessageRecipient { GroupName = groupName, ExcludedConnectionIds = excludedConnectionIds });
            await PublishAsync(message);
        }

        public override async Task SendGroupsAsync(IReadOnlyList<string> groupNames, string methodName, object[] args, CancellationToken cancellationToken = default(CancellationToken))
        {
            await _messageSender.SendGroupsAsync(groupNames, methodName, args, cancellationToken);
            var message = CreateMessage(methodName, args, new GroupsMessageRecipient { GroupNames = groupNames });
            await PublishAsync(message);
        }

        public override async Task SendUserAsync(string userId, string methodName, object[] args, CancellationToken cancellationToken = default(CancellationToken))
        {
            await _messageSender.SendUserAsync(userId, methodName, args, cancellationToken);
            var message = CreateMessage(methodName, args, new UserMessageRecipients { UserId = userId });
            await PublishAsync(message);
        }

        public override async Task SendUsersAsync(IReadOnlyList<string> userIds, string methodName, object[] args, CancellationToken cancellationToken = default(CancellationToken))
        {
            await _messageSender.SendUsersAsync(userIds, methodName, args, cancellationToken);
            var message = CreateMessage(methodName, args, new UsersMessageRecipients { UserIds = userIds });
            await PublishAsync(message);
        }

        private Task PublishAsync(SignalRMessage message)
        {
            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message, _options.SerializerSettings));
            var sbMessage = new Message(body);
            return _topicClient.SendAsync(sbMessage);
        }

        private SignalRMessage CreateMessage(string methodName, object[] args, params SignalRMessageRecipient[] recipients)
        {
            return new SignalRMessage
            {
                SenderId = _nodeIdProvider.NodeId,
                HubTypeName = typeof(THub).AssemblyQualifiedName,
                Method = methodName,
                Args = args,
                Recipients = recipients
            };
        }
    }
}
