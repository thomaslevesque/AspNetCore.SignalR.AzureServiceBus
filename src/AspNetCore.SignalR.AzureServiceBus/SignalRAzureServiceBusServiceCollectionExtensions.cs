using System;
using AspNetCore.SignalR.AzureServiceBus;
using AspNetCore.SignalR.AzureServiceBus.Internal;
using Microsoft.AspNetCore.SignalR;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SignalRAzureServiceBusServiceCollectionExtensions
    {
        public static ISignalRServerBuilder AddAzureServiceBus(this ISignalRServerBuilder builder, Action<SignalRAzureServiceBusOptions> configureOptions)
        {
            builder.Services.Configure(configureOptions);
            builder.Services.PostConfigure<SignalRAzureServiceBusOptions>(options =>
            {
                if (string.IsNullOrEmpty(options.ConnectionString))
                    throw new ArgumentException("ConnectionString must be specified");
                if (string.IsNullOrEmpty(options.TopicName))
                    throw new ArgumentException("TopicName must be specified");
                if (options.AutoDeleteSubscriptionOnIdle.TotalMinutes < 5)
                    throw new ArgumentException("AutoDeleteSubscriptionOnIdle must be at least 5 minutes");
                if (options.MessageTimeToLive.Ticks <= 0)
                    throw new ArgumentException("MessageTimeToLive must be strictly positive");
            });
            builder.Services.AddSingleton<INodeIdProvider, NodeIdProvider>();
            builder.Services.AddSingleton<IMessageSenderProvider, MessageSenderProvider>();
            builder.Services.AddSingleton<IServiceBusPublisher, ServiceBusPublisher>();
            builder.Services.AddSingleton(typeof(MessageSenderHubLifetimeManager<>));
            builder.Services.AddSingleton(typeof(HubLifetimeManager<>), typeof(AzureServiceBusHubLifetimeManager<>));
            builder.Services.AddHostedService<MessageDispatcher>();
            return builder;
        }
    }
}