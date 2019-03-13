using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace AspNetCore.SignalR.AzureServiceBus.Internal
{
    internal class ServiceBusListener : IServiceBusListener
    {
        private readonly INodeIdProvider _nodeIdProvider;
        private readonly SignalRAzureServiceBusOptions _options;
        private readonly ILogger<ServiceBusListener> _logger;

        private SubscriptionClient _subscriptionClient;

        public ServiceBusListener(
            INodeIdProvider nodeIdProvider,
            IOptions<SignalRAzureServiceBusOptions> options,
            ILogger<ServiceBusListener> logger)
        {
            _options = options.Value;
            _nodeIdProvider = nodeIdProvider;
            _logger = logger;
        }

        public async Task StartListeningAsync(Func<SignalRMessage, CancellationToken, Task> onMessageReceived, CancellationToken cancellationToken)
        {
            var subscriptionName = await GetOrCreateSubscriptionAsync(cancellationToken);
            _subscriptionClient = new SubscriptionClient(_options.ConnectionString, _options.TopicName, subscriptionName);
            _subscriptionClient.RegisterMessageHandler(
                (sbMessage, ct) => OnMessageReceived(sbMessage, onMessageReceived, ct),
                OnExceptionReceived);
        }

        public async Task StopListeningAsync(CancellationToken cancellationToken)
        {
            var subscriptionClient = _subscriptionClient;
            if (subscriptionClient != null)
            {
                ManagementClient managementClient = null;
                try
                {
                    managementClient = new ManagementClient(_options.ConnectionString);
                    await subscriptionClient.CloseAsync().WithCancellationToken(cancellationToken);
                    await managementClient.DeleteSubscriptionAsync(_subscriptionClient.TopicPath, _subscriptionClient.SubscriptionName, cancellationToken);
                }
                finally
                {
                    Interlocked.CompareExchange(ref _subscriptionClient, null, subscriptionClient);
                    if (managementClient != null)
                    {
                        await managementClient.CloseAsync().WithCancellationToken(cancellationToken);
                    }
                }
            }
        }

        private Task OnMessageReceived(Message sbMessage, Func<SignalRMessage, CancellationToken, Task> handler, CancellationToken cancellationToken)
        {
            var json = Encoding.UTF8.GetString(sbMessage.Body);
            try
            {
                var message = JsonConvert.DeserializeObject<SignalRMessage>(json, _options.SerializerSettings);
                if (!message.IsValid())
                {
                    _logger.LogWarning("Received invalid message: {MessageBody}", json);
                    return Task.CompletedTask;
                }

                return handler(message, cancellationToken);
            }
            catch (JsonSerializationException ex)
            {
                _logger.LogError(ex, "Failed to deserialize message: {MessageBody}", json);
                return Task.CompletedTask;
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
