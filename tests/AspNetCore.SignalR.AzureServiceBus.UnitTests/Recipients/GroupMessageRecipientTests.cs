using System;
using System.Threading;
using System.Threading.Tasks;
using AspNetCore.SignalR.AzureServiceBus.Internal.Recipients;
using Xunit;

namespace AspNetCore.SignalR.AzureServiceBus.UnitTests.Recipients
{
    public class GroupMessageRecipientTests
    {
        [Fact]
        public async Task SendCoreAsync_Sends_Message_To_Specified_Group()
        {
            string groupName = "abcd";

            await RecipientTestHelper.AssertSendAsync(
                new GroupMessageRecipient { GroupName = groupName },
                (sender, methodName, args) =>
                    () => sender.SendGroupExceptAsync(groupName, methodName, args, Array.Empty<string>(), CancellationToken.None));
        }

        [Fact]
        public async Task SendCoreAsync_Sends_Message_To_Specified_Group_Except_Excluded_Connections()
        {
            string groupName = "abcd";
            var excludedConnectionIds = new[] { "abcd", "efgh" };

            await RecipientTestHelper.AssertSendAsync(
                new GroupMessageRecipient { GroupName = groupName, ExcludedConnectionIds = excludedConnectionIds },
                (sender, methodName, args) =>
                    () => sender.SendGroupExceptAsync(groupName, methodName, args, excludedConnectionIds, CancellationToken.None));
        }
    }
}
