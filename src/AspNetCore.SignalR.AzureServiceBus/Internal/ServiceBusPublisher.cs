using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace AspNetCore.SignalR.AzureServiceBus.Internal
{
    internal class ServiceBusPublisher : IServiceBusPublisher
    {
        private readonly SignalRAzureServiceBusOptions _options;
        private readonly TopicClient _topicClient;

        public ServiceBusPublisher(IOptions<SignalRAzureServiceBusOptions> options)
        {
            _options = options.Value;
            _topicClient = new TopicClient(_options.ConnectionString, _options.TopicName);
        }

        public Task PublishAsync(SignalRMessage message)
        {
            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message, _options.SerializerSettings));
            var sbMessage = new Message(body);
            return _topicClient.SendAsync(sbMessage);
        }
    }
}