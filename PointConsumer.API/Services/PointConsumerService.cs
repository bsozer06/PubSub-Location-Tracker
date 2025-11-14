
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using PointConsumer.API.Configurations;
using PointConsumer.API.Constants;
using PointConsumer.API.Hubs;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;

namespace PointConsumer.API.Services
{
    public class PointConsumerService : IHostedService
    {
        private readonly string _uri;
        private IConnection? _connection;
        private IChannel? _channel;
        private readonly ILogger<PointConsumerService> _logger;
        private readonly IHubContext<PointHub> _pointHub;
        public PointConsumerService(
            ILogger<PointConsumerService> logger,
            IOptions<RabbitMQOptions> rabbitMqOptions,
            IHubContext<PointHub> pointHub)
        {
            _logger = logger;
            _pointHub = pointHub;
            _uri = rabbitMqOptions.Value.Uri;

            if (string.IsNullOrEmpty(_uri))
            {
                throw new ArgumentNullException(nameof(_uri), "RabbitMQ URI is not read.");
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                var factory = new ConnectionFactory() { Uri = new Uri(_uri) };
                _connection = await factory.CreateConnectionAsync();
                _channel = await _connection.CreateChannelAsync();

                await _channel.ExchangeDeclareAsync(
                    exchange: PointConsumerConstants.PointExchange,
                    type: ExchangeType.Fanout,
                    durable: true
                );

                var queueName = (await _channel.QueueDeclareAsync()).QueueName;
                await _channel.QueueBindAsync(
                    queue: queueName,
                    exchange: PointConsumerConstants.PointExchange,
                    routingKey: ""
                );

                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.ReceivedAsync += async (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = System.Text.Encoding.UTF8.GetString(body);
                    try
                    {
                        var points = JsonSerializer.Deserialize<Models.Point[]>(message);
                        _logger.LogInformation("The number of received points:" + points.Length.ToString());
                        if (points != null)
                        {
                            await _pointHub.Clients.All.SendAsync(PointConsumerConstants.ReceivePointUpdate, points);
                        }
                        await _channel.BasicAckAsync(ea.DeliveryTag, false);
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError(ex, "Failed to deserialize message: {Message}", message);
                        await _channel.BasicAckAsync(ea.DeliveryTag, false);
                    }
                };

                await _channel.BasicConsumeAsync(
                    queue: queueName,
                    autoAck: false,
                    consumer: consumer
                );

                _logger.LogInformation("Point Consumer Service started and listening for messages.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not start Point Consumer Service.");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Point Consumer Service is stopping.");
            _channel?.CloseAsync();
            _connection?.CloseAsync();
            return Task.CompletedTask;
        }
    }
}
