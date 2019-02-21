using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AspNetCore.SignalR.AzureServiceBus
{
    public class SignalRAzureServiceBusOptions
    {
        public string ConnectionString { get; set; }
        public string TopicName { get; set; }
        public TimeSpan AutoDeleteSubscriptionOnIdle { get; set; } = TimeSpan.FromHours(24);
        public TimeSpan MessageTimeToLive { get; set; } = TimeSpan.FromHours(1);

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