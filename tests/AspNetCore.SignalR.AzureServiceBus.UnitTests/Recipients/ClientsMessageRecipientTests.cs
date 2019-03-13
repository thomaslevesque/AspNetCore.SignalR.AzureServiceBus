using System.Threading;
using System.Threading.Tasks;
using AspNetCore.SignalR.AzureServiceBus.Internal.Recipients;
using Xunit;

namespace AspNetCore.SignalR.AzureServiceBus.UnitTests.Recipients
{
    public class ClientsMessageRecipientTests
    {
        [Fact]
        public async Task SendCoreAsync_Sends_Message_To_Specified_Clients()
        {
            var connectionIds = new[] { "abcd", "efgh" };

            await RecipientTestHelper.AssertSendAsync(
                new ClientsMessageRecipient { ConnectionIds = connectionIds },
                (sender, methodName, args) =>
                    () => sender.SendConnectionsAsync(connectionIds, methodName, args, CancellationToken.None));
        }
    }
}
