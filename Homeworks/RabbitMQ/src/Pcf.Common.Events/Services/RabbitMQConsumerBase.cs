using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pcf.Common.Events.Configuration;
using Pcf.Common.Events.Constants;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Pcf.Common.Events.Services
{
    /// <summary>
    /// Базовый класс для консюмеров RabbitMQ
    /// </summary>
    public abstract class RabbitMQConsumerBase : BackgroundService
    {
        private readonly RabbitMQSettings _settings;
        private readonly ILogger _logger;
        private IConnection? _connection;
        private IModel? _channel;

        protected RabbitMQConsumerBase(IOptions<RabbitMQSettings> settings, ILogger logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        protected abstract string QueueName { get; }
        protected abstract string RoutingKey { get; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(5000, stoppingToken); // Ждем запуска RabbitMQ

            try
            {
                InitializeRabbitMQ();
                StartConsuming(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при инициализации RabbitMQ консюмера");
                throw;
            }

            // Ждем сигнала остановки
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        private void InitializeRabbitMQ()
        {
            var factory = new ConnectionFactory()
            {
                HostName = _settings.Host,
                Port = _settings.Port,
                UserName = _settings.Username,
                Password = _settings.Password,
                VirtualHost = _settings.VirtualHost,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Объявляем exchange
            _channel.ExchangeDeclare(
                exchange: ExchangeNames.PromoCodeEvents,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false);

            // Объявляем очередь
            _channel.QueueDeclare(
                queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            // Привязываем очередь к exchange
            _channel.QueueBind(
                queue: QueueName,
                exchange: ExchangeNames.PromoCodeEvents,
                routingKey: RoutingKey);

            _logger.LogInformation($"RabbitMQ консюмер инициализирован для очереди {QueueName}");
        }

        private void StartConsuming(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);
            
            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    
                    _logger.LogInformation($"Получено сообщение: {message}");
                    
                    await ProcessMessageAsync(message);
                    
                    _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    _logger.LogInformation("Сообщение успешно обработано");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при обработке сообщения");
                    _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                }
            };

            _channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);
        }

        protected abstract Task ProcessMessageAsync(string message);

        public override void Dispose()
        {
            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
            base.Dispose();
        }
    }
} 