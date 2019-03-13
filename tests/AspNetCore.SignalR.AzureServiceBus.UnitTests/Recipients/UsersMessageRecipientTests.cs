using System.Threading;
using System.Threading.Tasks;
using AspNetCore.SignalR.AzureServiceBus.Internal.Recipients;
using Xunit;

namespace AspNetCore.SignalR.AzureServiceBus.UnitTests.Recipients
{
    public class UsersMessageRecipientTests
    {
        [Fact]
        public async Task SendCoreAsync_Sends_Message_To_Specified_Users()
        {
            var userIds = new[] { "abcd", "efgh" };

            await RecipientTestHelper.AssertSendAsync(
                new UsersMessageRecipient { UserIds = userIds },
                (sender, methodName, args) =>
                    () => sender.SendUsersAsync(userIds, methodName, args, CancellationToken.None));
        }
    }
}
