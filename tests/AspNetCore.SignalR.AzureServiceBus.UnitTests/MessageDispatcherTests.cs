using AspNetCore.SignalR.AzureServiceBus.Internal;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using MessageHandler = System.Func<AspNetCore.SignalR.AzureServiceBus.Internal.SignalRMessage, System.Threading.CancellationToken, System.Threading.Tasks.Task>;

namespace AspNetCore.SignalR.AzureServiceBus.UnitTests
{
    public class MessageDispatcherTests
    {
        // Fake dependencies
        private readonly INodeIdProvider _nodeIdProvider;
        private readonly IMessageSenderProvider _messageSenderProvider;
        private readonly IServiceBusListener _listener;
        private readonly ILogger<MessageDispatcher> _logger;
        private readonly IMessageSender _messageSender;

        // SUT
        private readonly MessageDispatcher _dispatcher;

        private MessageHandler _messageHandler;

        public MessageDispatcherTests()
        {
            _nodeIdProvider = A.Fake<INodeIdProvider>();
            _messageSenderProvider = A.Fake<IMessageSenderProvider>();
            _listener = A.Fake<IServiceBusListener>();
            _logger = A.Fake<ILogger<MessageDispatcher>>();
            _messageSender = A.Fake<IMessageSender>();

            A.CallTo(() => _nodeIdProvider.NodeId).Returns(Guid.NewGuid());

            A.CallTo(() => _messageSenderProvider.GetMessageSenderForHub(A<string>._)).Returns(_messageSender);

            A.CallTo(() => _listener.StartListeningAsync(A<MessageHandler>._, A<CancellationToken>._))
                .Invokes((MessageHandler mh, CancellationToken _) => _messageHandler = mh);

            _dispatcher = new MessageDispatcher(_nodeIdProvider, _messageSenderProvider, _listener, _logger);
        }

        [Fact]
        public async Task Messages_Are_Dispatched_To_Recipients()
        {
            await _dispatcher.StartAsync(CancellationToken.None);

            // Sanity check
            _messageHandler.Should().NotBeNull();

            var recipients = A.CollectionOfFake<SignalRMessageRecipient>(3);

            var message = new SignalRMessage
            {
                HubTypeName = "MyHub",
                SenderId = Guid.NewGuid(),
                Method = "Foo",
                Args = new object[] { "Hello", 42 },
                Recipients = recipients.ToArray()
            };

            await _messageHandler(message, CancellationToken.None);

            foreach (var recipient in recipients)
            {
                A.CallTo(() => recipient.SendCoreAsync(_messageSender, message.Method, message.Args, A<CancellationToken>._))
                    .MustHaveHappenedOnceExactly();
            }
        }

        [Fact]
        public async Task Messages_From_Self_Are_Not_Dispatched()
        {
            await _dispatcher.StartAsync(CancellationToken.None);

            // Sanity check
            _messageHandler.Should().NotBeNull();

            var recipients = A.CollectionOfFake<SignalRMessageRecipient>(3);

            var message = new SignalRMessage
            {
                HubTypeName = "MyHub",
                SenderId = _nodeIdProvider.NodeId,
                Method = "Foo",
                Args = new object[] { "Hello", 42 },
                Recipients = recipients.ToArray()
            };

            await _messageHandler(message, CancellationToken.None);

            foreach (var recipient in recipients)
            {
                A.CallTo(() => recipient.SendCoreAsync(default, default, default, default))
                    .WithAnyArguments()
                    .MustNotHaveHappened();
            }
        }

        [Fact]
        public async Task Messages_For_Unknown_Hub_Are_Not_Dispatched()
        {
            A.CallTo(() => _messageSenderProvider.GetMessageSenderForHub(A<string>._)).Returns(null);

            await _dispatcher.StartAsync(CancellationToken.None);

            // Sanity check
            _messageHandler.Should().NotBeNull();

            var recipients = A.CollectionOfFake<SignalRMessageRecipient>(3);

            var message = new SignalRMessage
            {
                HubTypeName = "MyHub",
                SenderId = Guid.NewGuid(),
                Method = "Foo",
                Args = new object[] { "Hello", 42 },
                Recipients = recipients.ToArray()
            };

            await _messageHandler(message, CancellationToken.None);

            foreach (var recipient in recipients)
            {
                A.CallTo(() => recipient.SendCoreAsync(default, default, default, default))
                    .WithAnyArguments()
                    .MustNotHaveHappened();
            }
        }

        [Fact]
        public async Task StopAsync_Stops_Listening()
        {
            await _dispatcher.StopAsync(CancellationToken.None);

            A.CallTo(() => _listener.StopListeningAsync(CancellationToken.None))
                .MustHaveHappenedOnceExactly();
        }
    }
}
