using IntEvents;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SignalR
{
    public class RabbitMqHub : Hub
    {
        public const string ReceiveLog = "ReceiveLog";

        private readonly IRabbitMQEventBus _eventBus;

        public RabbitMqHub(IRabbitMQEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public async Task CreateEvent(string description)
        {
            try
            {
                _eventBus.PublishMessage(description);
                await Clients.All.SendAsync(ReceiveLog, "Event published successfully.");
            }
            catch (Exception ex)
            {
                await Clients.All.SendAsync(ReceiveLog, $"Failed to publish RabbitMQ event. Exception: {ex.Message}");
            }
        }
    }

    public static class RabbitMqHubContextExtension
    {
        public static Task SendLogAsync(this IHubContext<RabbitMqHub> hubContext, string message) => hubContext.Clients.All.SendAsync(RabbitMqHub.ReceiveLog, message);
    }
}
