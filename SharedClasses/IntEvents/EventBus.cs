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
    public interface IEventBus
    {
        void PublishMessage(string description);
        void Subscribe();
    }

    public class EventBus : IEventBus, IDisposable
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _services;

        private IConnection _consumerConnection;
        private IModel _consumerChannel;

        public EventBus(IConfiguration configuration, IServiceProvider services)
        {
            _configuration = configuration;
            _services = services;
        }

        public void Dispose()
        {
            Log.Debug("Event bus disposed");
            if (_consumerChannel != null)
            {
                _consumerChannel.Dispose();
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
                // Serialize
                var json = JsonSerializer.Serialize(eventData);

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
            Log.Debug("Subscribed.");

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
                var json = Encoding.UTF8.GetString(body);

                Log.Debug($"Received message: [{json}].");

                var eventData = JsonSerializer.Deserialize<SampleEvent>(json);

                var hubContext = _services.GetService<IHubContext<RabbitMqHub>>();
                await hubContext.SendLogAsync($"Event received from [{eventData.From}] at [{eventData.Timestamp}] with content [{eventData.Description}].");
            };
            _consumerChannel.BasicConsume(queue: _configuration["ReceiveQueue"],
                                 autoAck: true,
                                 consumer: consumer);
        }
    }
}
