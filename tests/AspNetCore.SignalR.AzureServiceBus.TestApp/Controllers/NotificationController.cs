using System.Threading.Tasks;
using AspNetCore.SignalR.AzureServiceBus.TestApp.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace AspNetCore.SignalR.AzureServiceBus.TestApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly IHubContext<NotificationHub, INotificationClient> _hubContext;

        public NotificationController(IHubContext<NotificationHub, INotificationClient> hubContext)
        {
            _hubContext = hubContext;
        }

        [HttpPost]
        public async Task<IActionResult> SendNotificationAsync([FromBody] string notification)
        {
            await _hubContext.Clients.All.ReceiveNotification(notification);
            return Ok();
        }
    }
}
