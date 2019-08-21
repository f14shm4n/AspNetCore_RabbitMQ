using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;
using SignalR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace IntEvents
{
    // Event bus interface
    public interface IRabbitMQEventBus
    {
        void PublishMessage(string description);
        void Subscribe();
    }
    // Event bus impl
    public class RabbitMQEventBus : IRabbitMQEventBus, IDisposable
    {
        private readonly IConfiguration _configuration;
        private readonly IHubContext<RabbitMqHub> _hubContext;

        private IConnection _consumerConnection;
        private IModel _consumerChannel;

        public RabbitMQEventBus(IConfiguration configuration, IHubContext<RabbitMqHub> hubContext)
        {
            _configuration = configuration;
            _hubContext = hubContext;
        }

        public void Dispose()
        {
            if (_consumerChannel != null)
            {
                _consumerChannel.Dispose();
            }

            if (_consumerConnection != null)
            {
                _consumerConnection.Dispose();
            }
        }

        public void PublishMessage(string description)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using var connection = factory.CreateConnection();
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: _configuration["SendQueue"],
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);
                // Create our event data
                var eventData = new SampleEvent
                {
                    From = _configuration["ServiceName"],
                    Description = description
                };
                // Serialize event data
                var json = JsonSerializer.Serialize(eventData);
                // Print to debug
                Log.Debug($"Serialized data: [{json}]");

                var body = Encoding.UTF8.GetBytes(json);

                channel.BasicPublish(exchange: "",
                                     routingKey: _configuration["SendQueue"],
                                     basicProperties: null,
                                     body: body);
            }
        }

        public void Subscribe()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            _consumerConnection = factory.CreateConnection();
            _consumerChannel = _consumerConnection.CreateModel();
            _consumerChannel.QueueDeclare(queue: _configuration["ReceiveQueue"],
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var consumer = new EventingBasicConsumer(_consumerChannel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body;
                // Get our json event data
                var json = Encoding.UTF8.GetString(body);
                // Print data to debug
                Log.Debug($"Received message: [{json}].");
                // Deserialize json
                var eventData = JsonSerializer.Deserialize<SampleEvent>(json);
                // Notify SignalR clients
                await _hubContext.SendLogAsync($"Event received from [{eventData.From}] at [{eventData.Timestamp}] with content [{eventData.Description}].");
            };
            _consumerChannel.BasicConsume(queue: _configuration["ReceiveQueue"],
                                 autoAck: true,
                                 consumer: consumer);
        }
    }
}
