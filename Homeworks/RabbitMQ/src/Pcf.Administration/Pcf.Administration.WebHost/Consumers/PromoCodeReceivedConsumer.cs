using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pcf.Administration.WebHost.EventHandlers;
using Pcf.Common.Events.Configuration;
using Pcf.Common.Events.Constants;
using Pcf.Common.Events.Events;
using Pcf.Common.Events.Services;

namespace Pcf.Administration.WebHost.Consumers
{
    /// <summary>
    /// Консюмер для обработки событий получения промокодов
    /// </summary>
    public class PromoCodeReceivedConsumer : RabbitMQConsumerBase
    {
        private readonly IServiceProvider _serviceProvider;

        public PromoCodeReceivedConsumer(
            IOptions<RabbitMQSettings> settings,
            ILogger<PromoCodeReceivedConsumer> logger,
            IServiceProvider serviceProvider)
            : base(settings, logger)
        {
            _serviceProvider = serviceProvider;
        }

        protected override string QueueName => QueueNames.PromoCodeReceivedAdministration;
        protected override string RoutingKey => RoutingKeys.PromoCodeReceived;

        protected override async Task ProcessMessageAsync(string message)
        {
            var eventData = JsonConvert.DeserializeObject<PromoCodeReceivedEvent>(message);
            if (eventData != null)
            {
                using var scope = _serviceProvider.CreateScope();
                var eventHandler = scope.ServiceProvider.GetRequiredService<PromoCodeReceivedEventHandler>();
                await eventHandler.HandleAsync(eventData);
            }
        }
    }
} 