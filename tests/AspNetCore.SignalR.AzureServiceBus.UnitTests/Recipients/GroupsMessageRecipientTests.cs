using System.Threading;
using System.Threading.Tasks;
using AspNetCore.SignalR.AzureServiceBus.Internal.Recipients;
using Xunit;

namespace AspNetCore.SignalR.AzureServiceBus.UnitTests.Recipients
{
    public class GroupsMessageRecipientTests
    {
        [Fact]
        public async Task SendCoreAsync_Sends_Message_To_Specified_Groups()
        {
            var groupNames = new[] { "abcd", "efgh" };

            await RecipientTestHelper.AssertSendAsync(
                new GroupsMessageRecipient { GroupNames = groupNames },
                (sender, methodName, args) =>
                    () => sender.SendGroupsAsync(groupNames, methodName, args, CancellationToken.None));
        }
    }
}
