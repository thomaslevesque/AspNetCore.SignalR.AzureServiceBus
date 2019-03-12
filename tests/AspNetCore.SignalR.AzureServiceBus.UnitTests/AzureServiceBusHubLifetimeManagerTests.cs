using System;
using Xunit;
using Microsoft.AspNetCore.SignalR;
using AspNetCore.SignalR.AzureServiceBus.Internal;
using FakeItEasy;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using AspNetCore.SignalR.AzureServiceBus.Internal.Recipients;
using FluentAssertions;

namespace AspNetCore.SignalR.AzureServiceBus.UnitTests
{
    public class AzureServiceBusHubLifetimeManagerTests
    {
        private readonly IServiceBusPublisher _serviceBusPublisher;
        private readonly INodeIdProvider _nodeIdProvider;
        private readonly MessageSenderHubLifetimeManager<TestHub> _messageSender;
        private readonly AzureServiceBusHubLifetimeManager<TestHub> _lifetimeManager;

        public AzureServiceBusHubLifetimeManagerTests()
        {
            _serviceBusPublisher = A.Fake<IServiceBusPublisher>();
            _nodeIdProvider = A.Fake<INodeIdProvider>();
            _messageSender = A.Fake<MessageSenderHubLifetimeManager<TestHub>>();
            _lifetimeManager = new AzureServiceBusHubLifetimeManager<TestHub>(_nodeIdProvider, _messageSender, _serviceBusPublisher);

            A.CallTo(() => _nodeIdProvider.NodeId).Returns(Guid.NewGuid());
        }

        [Fact]
        public async Task AddToGroupAsync_Delegates_To_Message_Sender()
        {
            string connectionId = Guid.NewGuid().ToString();
            string groupName = Guid.NewGuid().ToString();
            await _lifetimeManager.AddToGroupAsync(connectionId, groupName, CancellationToken.None);

            A.CallTo(() => _messageSender.AddToGroupAsync(connectionId, groupName, CancellationToken.None))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task RemoveFromGroupAsync_Delegates_To_Message_Sender()
        {
            string connectionId = Guid.NewGuid().ToString();
            string groupName = Guid.NewGuid().ToString();
            await _lifetimeManager.RemoveFromGroupAsync(connectionId, groupName, CancellationToken.None);

            A.CallTo(() => _messageSender.RemoveFromGroupAsync(connectionId, groupName, CancellationToken.None))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task OnConnectedAsync_Delegates_To_Message_Sender()
        {
            var context = A.Dummy<HubConnectionContext>();
            await _lifetimeManager.OnConnectedAsync(context);

            A.CallTo(() => _messageSender.OnConnectedAsync(context))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task OnDisconnectedAsync_Delegates_To_Message_Sender()
        {
            var context = A.Dummy<HubConnectionContext>();
            await _lifetimeManager.OnDisconnectedAsync(context);

            A.CallTo(() => _messageSender.OnDisconnectedAsync(context))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task SendAllAsync_Delegates_To_Message_Sender_And_Publishes_Message_To_All_Recipients()
        {
            string methodName = Guid.NewGuid().ToString();
            var args = new object[] { 42, "hello world" };

            SignalRMessage message = null;
            A.CallTo(() => _serviceBusPublisher.PublishAsync(A<SignalRMessage>._))
                .Invokes((SignalRMessage m) => message = m);

            await _lifetimeManager.SendAllAsync(methodName, args, CancellationToken.None);

            A.CallTo(() => _messageSender.SendAllAsync(methodName, args, CancellationToken.None))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _serviceBusPublisher.PublishAsync(A<SignalRMessage>._))
                .MustHaveHappenedOnceExactly();

            ValidateCommonMessageProperties(message);
            ValidateMethodAndArgs(message, methodName, args);
            ValidateSingleRecipient<AllMessageRecipient>(message, r => r.ExcludedConnectionIds.Should().BeNullOrEmpty());
        }

        [Fact]
        public async Task SendAllExceptAsync_Delegates_To_Message_Sender_And_Publishes_Message_To_All_Recipients_Except_Specified()
        {
            string methodName = Guid.NewGuid().ToString();
            var args = new object[] { 42, "hello world" };
            var excludedConnectionIds = new[] { Guid.NewGuid().ToString() };

            SignalRMessage message = null;
            A.CallTo(() => _serviceBusPublisher.PublishAsync(A<SignalRMessage>._))
                .Invokes((SignalRMessage m) => message = m);

            await _lifetimeManager.SendAllExceptAsync(methodName, args, excludedConnectionIds, CancellationToken.None);

            A.CallTo(() => _messageSender.SendAllExceptAsync(methodName, args, excludedConnectionIds, CancellationToken.None))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _serviceBusPublisher.PublishAsync(A<SignalRMessage>._))
                .MustHaveHappenedOnceExactly();

            ValidateCommonMessageProperties(message);
            ValidateMethodAndArgs(message, methodName, args);
            ValidateSingleRecipient<AllMessageRecipient>(message, r => r.ExcludedConnectionIds.Should().Equal(excludedConnectionIds));
        }

        [Fact]
        public async Task SendConnectionAsync_Delegates_To_Message_Sender_And_Publishes_Message_To_Client_Recipient()
        {
            string methodName = Guid.NewGuid().ToString();
            var args = new object[] { 42, "hello world" };
            var connectionId = Guid.NewGuid().ToString();

            SignalRMessage message = null;
            A.CallTo(() => _serviceBusPublisher.PublishAsync(A<SignalRMessage>._))
                .Invokes((SignalRMessage m) => message = m);

            await _lifetimeManager.SendConnectionAsync(connectionId, methodName, args, CancellationToken.None);

            A.CallTo(() => _messageSender.SendConnectionAsync(connectionId, methodName, args, CancellationToken.None))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _serviceBusPublisher.PublishAsync(A<SignalRMessage>._))
                .MustHaveHappenedOnceExactly();

            ValidateCommonMessageProperties(message);
            ValidateMethodAndArgs(message, methodName, args);
            ValidateSingleRecipient<ClientMessageRecipient>(message, r => r.ConnectionId.Should().Be(connectionId));
        }

        [Fact]
        public async Task SendConnectionsAsync_Delegates_To_Message_Sender_And_Publishes_Message_To_Client_Recipients()
        {
            string methodName = Guid.NewGuid().ToString();
            var args = new object[] { 42, "hello world" };
            var connectionIds = new[] { Guid.NewGuid().ToString(), Guid.NewGuid().ToString() };

            SignalRMessage message = null;
            A.CallTo(() => _serviceBusPublisher.PublishAsync(A<SignalRMessage>._))
                .Invokes((SignalRMessage m) => message = m);

            await _lifetimeManager.SendConnectionsAsync(connectionIds, methodName, args, CancellationToken.None);

            A.CallTo(() => _messageSender.SendConnectionsAsync(connectionIds, methodName, args, CancellationToken.None))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _serviceBusPublisher.PublishAsync(A<SignalRMessage>._))
                .MustHaveHappenedOnceExactly();

            ValidateCommonMessageProperties(message);
            ValidateMethodAndArgs(message, methodName, args);
            ValidateSingleRecipient<ClientsMessageRecipient>(message, r => r.ConnectionIds.Should().Equal(connectionIds));
        }

        [Fact]
        public async Task SendGroupAsync_Delegates_To_Message_Sender_And_Publishes_Message_To_Group_Recipient()
        {
            string methodName = Guid.NewGuid().ToString();
            var args = new object[] { 42, "hello world" };
            var groupName = "mygroup";

            SignalRMessage message = null;
            A.CallTo(() => _serviceBusPublisher.PublishAsync(A<SignalRMessage>._))
                .Invokes((SignalRMessage m) => message = m);

            await _lifetimeManager.SendGroupAsync(groupName, methodName, args, CancellationToken.None);

            A.CallTo(() => _messageSender.SendGroupAsync(groupName, methodName, args, CancellationToken.None))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _serviceBusPublisher.PublishAsync(A<SignalRMessage>._))
                .MustHaveHappenedOnceExactly();

            ValidateCommonMessageProperties(message);
            ValidateMethodAndArgs(message, methodName, args);
            ValidateSingleRecipient<GroupMessageRecipient>(message, r => r.GroupName.Should().Be(groupName));
        }

        [Fact]
        public async Task SendGroupExceptAsync_Delegates_To_Message_Sender_And_Publishes_Message_To_Group_Recipient_Except_Specified()
        {
            string methodName = Guid.NewGuid().ToString();
            var args = new object[] { 42, "hello world" };
            var groupName = "mygroup";
            var excludedConnectionIds = new[] { Guid.NewGuid().ToString() };

            SignalRMessage message = null;
            A.CallTo(() => _serviceBusPublisher.PublishAsync(A<SignalRMessage>._))
                .Invokes((SignalRMessage m) => message = m);

            await _lifetimeManager.SendGroupExceptAsync(groupName, methodName, args, excludedConnectionIds, CancellationToken.None);

            A.CallTo(() => _messageSender.SendGroupExceptAsync(groupName, methodName, args, excludedConnectionIds, CancellationToken.None))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _serviceBusPublisher.PublishAsync(A<SignalRMessage>._))
                .MustHaveHappenedOnceExactly();

            ValidateCommonMessageProperties(message);
            ValidateMethodAndArgs(message, methodName, args);
            ValidateSingleRecipient<GroupMessageRecipient>(message, r =>
            {
                r.GroupName.Should().Be(groupName);
                r.ExcludedConnectionIds.Should().Equal(excludedConnectionIds);
            });
        }

        [Fact]
        public async Task SendGroupsAsync_Delegates_To_Message_Sender_And_Publishes_Message_To_Group_Recipients()
        {
            string methodName = Guid.NewGuid().ToString();
            var args = new object[] { 42, "hello world" };
            var groupNames = new[] { "mygroup1", "mygroup2" };

            SignalRMessage message = null;
            A.CallTo(() => _serviceBusPublisher.PublishAsync(A<SignalRMessage>._))
                .Invokes((SignalRMessage m) => message = m);

            await _lifetimeManager.SendGroupsAsync(groupNames, methodName, args, CancellationToken.None);

            A.CallTo(() => _messageSender.SendGroupsAsync(groupNames, methodName, args, CancellationToken.None))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _serviceBusPublisher.PublishAsync(A<SignalRMessage>._))
                .MustHaveHappenedOnceExactly();

            ValidateCommonMessageProperties(message);
            ValidateMethodAndArgs(message, methodName, args);
            ValidateSingleRecipient<GroupsMessageRecipient>(message, r => r.GroupNames.Should().Equal(groupNames));
        }

        [Fact]
        public async Task SendUserAsync_Delegates_To_Message_Sender_And_Publishes_Message_To_User_Recipient()
        {
            string methodName = Guid.NewGuid().ToString();
            var args = new object[] { 42, "hello world" };
            var userId = Guid.NewGuid().ToString();

            SignalRMessage message = null;
            A.CallTo(() => _serviceBusPublisher.PublishAsync(A<SignalRMessage>._))
                .Invokes((SignalRMessage m) => message = m);

            await _lifetimeManager.SendUserAsync(userId, methodName, args, CancellationToken.None);

            A.CallTo(() => _messageSender.SendUserAsync(userId, methodName, args, CancellationToken.None))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _serviceBusPublisher.PublishAsync(A<SignalRMessage>._))
                .MustHaveHappenedOnceExactly();

            ValidateCommonMessageProperties(message);
            ValidateMethodAndArgs(message, methodName, args);
            ValidateSingleRecipient<UserMessageRecipient>(message, r => r.UserId.Should().Be(userId));
        }

        [Fact]
        public async Task SendUsersAsync_Delegates_To_Message_Sender_And_Publishes_Message_To_User_Recipients()
        {
            string methodName = Guid.NewGuid().ToString();
            var args = new object[] { 42, "hello world" };
            var userIds = new[] { Guid.NewGuid().ToString(), Guid.NewGuid().ToString() };

            SignalRMessage message = null;
            A.CallTo(() => _serviceBusPublisher.PublishAsync(A<SignalRMessage>._))
                .Invokes((SignalRMessage m) => message = m);

            await _lifetimeManager.SendUsersAsync(userIds, methodName, args, CancellationToken.None);

            A.CallTo(() => _messageSender.SendUsersAsync(userIds, methodName, args, CancellationToken.None))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _serviceBusPublisher.PublishAsync(A<SignalRMessage>._))
                .MustHaveHappenedOnceExactly();

            ValidateCommonMessageProperties(message);
            ValidateMethodAndArgs(message, methodName, args);
            ValidateSingleRecipient<UsersMessageRecipient>(message, r => r.UserIds.Should().Equal(userIds));
        }

        private void ValidateCommonMessageProperties(SignalRMessage message)
        {
            message.SenderId.Should().Be(_nodeIdProvider.NodeId);
            message.HubTypeName.Should().Be(typeof(TestHub).AssemblyQualifiedName);
        }

        private void ValidateMethodAndArgs(SignalRMessage message, string methodName, object[] args)
        {
            message.Method.Should().Be(methodName);
            message.Args.Should().Equal(args);
        }

        private void ValidateSingleRecipient<TRecipient>(SignalRMessage message, Action<TRecipient> recipientAssertions = null)
        {
            message.Recipients.Should().HaveCount(1);
            var recipient = message.Recipients.Single();
            var typedRecipient = recipient.Should().BeOfType<TRecipient>().Subject;
            recipientAssertions?.Invoke(typedRecipient);
        }

        public class TestHub : Hub
        {
        }
    }
}
