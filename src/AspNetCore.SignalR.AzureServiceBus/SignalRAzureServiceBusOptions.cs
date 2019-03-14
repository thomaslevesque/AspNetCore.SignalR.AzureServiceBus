using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AspNetCore.SignalR.AzureServiceBus
{
    /// <summary>
    /// Options used to configure SignalR scale-out with Azure Service Bus.
    /// </summary>
    public class SignalRAzureServiceBusOptions
    {
        /// <summary>
        /// The service bus connection string
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// The service bus topic name. This must be an existing topic.
        /// </summary>
        public string TopicName { get; set; }

        /// <summary>
        /// The delay after which an idle topic subscription should be deleted. The default is 24h.
        /// </summary>
        public TimeSpan AutoDeleteSubscriptionOnIdle { get; set; } = TimeSpan.FromHours(24);

        /// <summary>
        /// The time to live for messages in the subscription. The default is 1h.
        /// </summary>
        public TimeSpan MessageTimeToLive { get; set; } = TimeSpan.FromHours(1);

        /// <summary>
        /// The JSON serializer settings.
        /// </summary>
        public JsonSerializerSettings SerializerSettings { get; set; } = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            Converters =
            {
                new StringEnumConverter()
            }
        };
    }
}