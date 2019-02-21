using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace AspNetCore.SignalR.AzureServiceBus.TestApi.Hubs
{
    public class NotificationHub : Hub<INotificationClient>
    {
    }

    public interface INotificationClient
    {
        Task ReceiveNotification(string notification);
    }
}
