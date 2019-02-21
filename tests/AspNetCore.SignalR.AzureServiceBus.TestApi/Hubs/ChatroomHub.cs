using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace AspNetCore.SignalR.AzureServiceBus.TestApi.Hubs
{
    public class ChatroomHub : Hub<IChatroomClient>
    {
        public async Task SendMessageAsync(string message, CancellationToken cancellationToken)
        {
            await Clients.All.ReceiveMessage(message, cancellationToken);
        }
    }

    public interface IChatroomClient
    {
        Task ReceiveMessage(string message, CancellationToken cancellationToken);
    }
}
