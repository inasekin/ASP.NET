using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pcf.Common.Events.Abstractions;
using Pcf.Common.Events.Configuration;
using Pcf.Common.Events.Constants;
using RabbitMQ.Client;

namespace Pcf.Common.Events.Services
{
    /// <summary>
    /// Реализация публикатора событий для RabbitMQ
    /// </summary>
    public class RabbitMQEventPublisher : IEventPublisher, IDisposable
    {
        private readonly RabbitMQSettings _settings;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RabbitMQEventPublisher(IOptions<RabbitMQSettings> settings)
        {
            _settings = settings.Value;
            
            var factory = new ConnectionFactory()
            {
                HostName = _settings.Host,
                Port = _settings.Port,
                UserName = _settings.Username,
                Password = _settings.Password,
                VirtualHost = _settings.VirtualHost
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Объявляем exchange
            _channel.ExchangeDeclare(
                exchange: ExchangeNames.PromoCodeEvents,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false);
        }

        public async Task PublishAsync<T>(T eventData, string routingKey) where T : class
        {
            var message = JsonConvert.SerializeObject(eventData);
            var body = Encoding.UTF8.GetBytes(message);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.MessageId = Guid.NewGuid().ToString();
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            _channel.BasicPublish(
                exchange: ExchangeNames.PromoCodeEvents,
                routingKey: routingKey,
                basicProperties: properties,
                body: body);

            await Task.CompletedTask;
        }

        public void Dispose()
        {
            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
        }
    }
} 