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
        private readonly ILogger<MessageDispatcher> _logger;
        private readonly SignalRAzureServiceBusOptions _options;

        private SubscriptionClient _subscriptionClient;

        public MessageDispatcher(
            INodeIdProvider nodeIdProvider,
            IMessageSenderProvider messageSenderProvider,
            IOptions<SignalRAzureServiceBusOptions> options,
            ILogger<MessageDispatcher> logger)
        {
            _nodeIdProvider = nodeIdProvider;
            _messageSenderProvider = messageSenderProvider;
            _options = options.Value;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var subscriptionName = await GetOrCreateSubscriptionAsync(cancellationToken);
            _subscriptionClient = new SubscriptionClient(_options.ConnectionString, _options.TopicName, subscriptionName);
            _subscriptionClient.RegisterMessageHandler(OnMessageReceived, OnExceptionReceived);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_subscriptionClient != null)
            {
                ManagementClient managementClient = null;
                try
                {
                    managementClient = new ManagementClient(_options.ConnectionString);
                    await _subscriptionClient.CloseAsync().WithCancellationToken(cancellationToken);
                    await managementClient.DeleteSubscriptionAsync(_subscriptionClient.TopicPath, _subscriptionClient.SubscriptionName, cancellationToken);
                }
                finally
                {
                    _subscriptionClient = null;
                    if (managementClient != null)
                    {
                        await managementClient.CloseAsync().WithCancellationToken(cancellationToken);
                    }
                }
            }
        }

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

        private async Task<string> GetOrCreateSubscriptionAsync(CancellationToken cancellationToken)
        {
            var managementClient = new ManagementClient(_options.ConnectionString);
            string subscriptionName = $"sub-{_nodeIdProvider.NodeId}";
            try
            {
                await managementClient.GetSubscriptionAsync(_options.TopicName, subscriptionName, cancellationToken);
            }
            catch (MessagingEntityNotFoundException)
            {
                var subscription = new SubscriptionDescription(_options.TopicName, subscriptionName)
                {
                    AutoDeleteOnIdle = _options.AutoDeleteSubscriptionOnIdle,
                    DefaultMessageTimeToLive = _options.MessageTimeToLive
                };
                await managementClient.CreateSubscriptionAsync(subscription, cancellationToken);
            }

            return subscriptionName;
        }
    }
}