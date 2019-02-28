using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.Options;

namespace AspNetCore.SignalR.AzureServiceBus.Internal
{
    internal class ServiceBusListener : IServiceBusListener
    {
        private readonly INodeIdProvider _nodeIdProvider;
        private readonly SignalRAzureServiceBusOptions _options;

        private SubscriptionClient _subscriptionClient;

        public ServiceBusListener(INodeIdProvider nodeIdProvider, IOptions<SignalRAzureServiceBusOptions> options)
        {
            _options = options.Value;
            _nodeIdProvider = nodeIdProvider;
        }

        public async Task StartListeningAsync(Func<Message, CancellationToken, Task> onMessageReceived, Func<ExceptionReceivedEventArgs, Task> onError, CancellationToken cancellationToken)
        {
            var subscriptionName = await GetOrCreateSubscriptionAsync(cancellationToken);
            _subscriptionClient = new SubscriptionClient(_options.ConnectionString, _options.TopicName, subscriptionName);
            _subscriptionClient.RegisterMessageHandler(onMessageReceived, onError);
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