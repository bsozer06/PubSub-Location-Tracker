using RabbitMQ.Client;
using System.Text.Json;
using System.Text;
using PointProducer.API.Models;
using Microsoft.Extensions.Options;
using PointProducer.API.Configurations;

namespace PointProducer.API.Services
{
    public class PointPublisherService
    {
        private readonly string _uri;

        public PointPublisherService(IOptions<RabbitMQOptions> rabbitMQOptions)
        {
            _uri = rabbitMQOptions.Value.Uri;

            if (string.IsNullOrEmpty(_uri))
            {
                throw new ArgumentNullException(nameof(_uri), "RabbitMQ URI is not read.");
            }
        }

        public async Task PublishPointChangeAsync(Point[] points)
        {
            var factory = new ConnectionFactory();
            factory.Uri = new Uri(_uri);

            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            await channel.ExchangeDeclareAsync(
                exchange: "point_changes",
                type: ExchangeType.Fanout,
                durable: true
            );

            var message = points;

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            // Create an instance of BasicProperties that implements IAmqpHeader  
            var props = new BasicProperties();

            await channel.BasicPublishAsync(
                exchange: "point_changes",
                routingKey: "",
                mandatory: false,
                basicProperties: props,
                body: body
            );
        }
    }
}
