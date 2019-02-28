using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace AspNetCore.SignalR.AzureServiceBus.Internal
{
    internal class MessageDispatcher : IHostedService
    {
        private readonly INodeIdProvider _nodeIdProvider;
        private readonly IMessageSenderProvider _messageSenderProvider;
        private readonly IServiceBusListener _serviceBusListener;
        private readonly ILogger<MessageDispatcher> _logger;
        private readonly SignalRAzureServiceBusOptions _options;

        public MessageDispatcher(
            INodeIdProvider nodeIdProvider,
            IMessageSenderProvider messageSenderProvider,
            IServiceBusListener serviceBusListener,
            IOptions<SignalRAzureServiceBusOptions> options,
            ILogger<MessageDispatcher> logger)
        {
            _nodeIdProvider = nodeIdProvider;
            _messageSenderProvider = messageSenderProvider;
            _serviceBusListener = serviceBusListener;
            _options = options.Value;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken) =>
            _serviceBusListener.StartListeningAsync(OnMessageReceived, OnExceptionReceived, cancellationToken);

        public Task StopAsync(CancellationToken cancellationToken) =>
            _serviceBusListener.StopListeningAsync(cancellationToken);

        private async Task OnMessageReceived(Message sbMessage, CancellationToken cancellationToken)
        {
            var json = Encoding.UTF8.GetString(sbMessage.Body);
            SignalRMessage message = JsonConvert.DeserializeObject<SignalRMessage>(json, _options.SerializerSettings);

            if (!message.IsValid())
            {
                _logger.LogWarning("Received invalid message: {MessageBody}", sbMessage.Body);
                return;
            }

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

        private Task OnExceptionReceived(ExceptionReceivedEventArgs e)
        {
            _logger.LogError(e.Exception, "Error processing message");
            return Task.CompletedTask;
        }
    }
}