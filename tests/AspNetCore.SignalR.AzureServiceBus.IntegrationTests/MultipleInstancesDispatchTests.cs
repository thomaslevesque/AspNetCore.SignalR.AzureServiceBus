using AspNetCore.SignalR.AzureServiceBus.TestApi;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AspNetCore.SignalR.AzureServiceBus.IntegrationTests
{
    public class MultipleInstancesDispatchTests
    {
        [Fact]
        public async Task Clients_Receive_Messages_Sent_From_Other_Server_Instance()
        {
            TestServer
                server1 = CreateServer("https://localhost:51500"),
                server2 = CreateServer("https://localhost:51501");

            HubConnection
                client1 = CreateClient(server1),
                client2 = CreateClient(server2);

            // Use semaphore instead of manual reset event because
            // it supports async
            SemaphoreSlim
                client1ReceivedMessage = new SemaphoreSlim(0, 1),
                client2ReceivedMessage = new SemaphoreSlim(0, 1);

            string messageReceivedByClient1 = null, messageReceivedByClient2 = null;

            client1.On<string>("ReceiveMessage", message =>
            {
                messageReceivedByClient1 = message;
                client1ReceivedMessage.Release();
            });

            client2.On<string>("ReceiveMessage", message =>
            {
                messageReceivedByClient2 = message;
                client2ReceivedMessage.Release();
            });

            await client1.StartAsync();
            await client2.StartAsync();

            string messageFromClient1 = "Hello world from 1";
            string messageFromClient2 = "Hello world from 2";
            await client1.SendAsync("SendMessageAsync", messageFromClient1);
            await client2.SendAsync("SendMessageAsync", messageFromClient2);

            await Task.WhenAll(
                client1ReceivedMessage.WaitAsync(5000),
                client2ReceivedMessage.WaitAsync(5000));

            messageReceivedByClient1.Should().Be(messageFromClient2);
            messageReceivedByClient2.Should().Be(messageFromClient1);
        }

        private TestServer CreateServer(string url)
        {
            var builder = new WebHostBuilder()
                //.UseUrls(url)
                .UseStartup<Startup>()
                .ConfigureAppConfiguration(config => config.AddUserSecrets<Startup>())
                .ConfigureLogging(logging => logging.AddDebug());

            return new TestServer(builder);
        }

        private HubConnection CreateClient(TestServer server)
        {
            return new HubConnectionBuilder()
                .WithUrl(
                    new Uri(server.BaseAddress, "/hub/chat"),
                    options =>
                    {
                        // TestServer is in-memory, we can't really connect
                        // via HTTP. Use LongPolling and TestServer's handler
                        // to send requests instead.
                        options.Transports = HttpTransportType.LongPolling;
                        options.HttpMessageHandlerFactory = _ => server.CreateHandler();
                    })
                .ConfigureLogging(logging => logging.AddDebug())
                .Build();
        }
    }
}
