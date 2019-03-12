using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.SignalR.AzureServiceBus.Internal
{
    internal abstract class SignalRMessageRecipient
    {
        public abstract Task SendCoreAsync(IMessageSender sender, string method, object[] args, CancellationToken cancellationToken);
    }
}
