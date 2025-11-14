using Microsoft.AspNetCore.SignalR;
using PointConsumer.API.Constants;
using PointConsumer.API.Models;

namespace PointConsumer.API.Hubs
{
    public class PointHub : Hub
    {
        public async Task SendPointUpdate(Point point)
        {
            await Clients.All.SendAsync(PointConsumerConstants.ReceivePointUpdate, point);
        }
    }
}
