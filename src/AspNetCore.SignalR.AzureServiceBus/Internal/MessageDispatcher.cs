using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AspNetCore.SignalR.AzureServiceBus.Internal
{
    internal class MessageDispatcher : IHostedService
    {
        private readonly INodeIdProvider _nodeIdProvider;
        private readonly IMessageSenderProvider _messageSenderProvider;
        private readonly IServiceBusListener _serviceBusListener;
        private readonly ILogger<MessageDispatcher> _logger;

        public MessageDispatcher(
            INodeIdProvider nodeIdProvider,
            IMessageSenderProvider messageSenderProvider,
            IServiceBusListener serviceBusListener,
            ILogger<MessageDispatcher> logger)
        {
            _nodeIdProvider = nodeIdProvider;
            _messageSenderProvider = messageSenderProvider;
            _serviceBusListener = serviceBusListener;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken) =>
            _serviceBusListener.StartListeningAsync(OnMessageReceived, cancellationToken);

        public Task StopAsync(CancellationToken cancellationToken) =>
            _serviceBusListener.StopListeningAsync(cancellationToken);

        private async Task OnMessageReceived(SignalRMessage message, CancellationToken cancellationToken)
        {
            if (message.SenderId == _nodeIdProvider.NodeId)
            {
                // Ignore messages sent by self
                return;
            }

            var messageSender = _messageSenderProvider.GetMessageSenderForHub(message.HubTypeName);
            if (messageSender == null)
            {
                _logger.LogWarning("Can't find message sender for hub '{HubTypeName}'", message.HubTypeName);
                return;
            }

            foreach(var recipient in message.Recipients)
            {
                await recipient.SendCoreAsync(messageSender, message.Method, message.Args, cancellationToken);
            }
        }
    }
}
