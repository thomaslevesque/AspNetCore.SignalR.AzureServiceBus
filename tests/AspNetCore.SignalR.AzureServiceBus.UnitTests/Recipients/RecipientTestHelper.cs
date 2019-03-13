using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AspNetCore.SignalR.AzureServiceBus.Internal;
using AspNetCore.SignalR.AzureServiceBus.Internal.Recipients;
using FakeItEasy;
using Xunit;

namespace AspNetCore.SignalR.AzureServiceBus.UnitTests.Recipients
{
    internal class RecipientTestHelper
    {
        public static async Task AssertSendAsync<TRecipient>(
            TRecipient recipient,
            Func<IMessageSender, string, object[], Expression<Func<Task>>> getExpressionToAssert)
            where TRecipient : SignalRMessageRecipient
        {
            var sender = A.Fake<IMessageSender>();
            var methodName = "Foo";
            var args = new object[] { "hello", 42 };

            await recipient.SendCoreAsync(sender, methodName, args, CancellationToken.None);

            var expressionToAssert = getExpressionToAssert(sender, methodName, args);
            A.CallTo(expressionToAssert).MustHaveHappenedOnceExactly();
        }
    }
}