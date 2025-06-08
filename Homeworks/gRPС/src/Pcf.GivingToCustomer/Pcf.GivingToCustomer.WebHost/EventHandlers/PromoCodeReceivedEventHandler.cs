using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pcf.GivingToCustomer.Core.Services;
using Pcf.Common.Events.Abstractions;
using Pcf.Common.Events.Events;

namespace Pcf.GivingToCustomer.WebHost.EventHandlers
{
    /// <summary>
    /// Обработчик событий получения промокодов для выдачи клиентам
    /// </summary>
    public class PromoCodeReceivedEventHandler : IEventHandler<PromoCodeReceivedEvent>
    {
        private readonly IPromoCodeService _promoCodeService;
        private readonly ILogger<PromoCodeReceivedEventHandler> _logger;

        public PromoCodeReceivedEventHandler(
            IPromoCodeService promoCodeService,
            ILogger<PromoCodeReceivedEventHandler> logger)
        {
            _promoCodeService = promoCodeService;
            _logger = logger;
        }

        public async Task HandleAsync(PromoCodeReceivedEvent eventData)
        {
            _logger.LogInformation($"Обработка события получения промокода для выдачи клиентам: {JsonConvert.SerializeObject(eventData)}");

            try
            {
                await _promoCodeService.GivePromoCodeToCustomersAsync(
                    eventData.PromoCodeId,
                    eventData.Code,
                    eventData.ServiceInfo,
                    eventData.BeginDate,
                    eventData.EndDate,
                    eventData.PartnerId,
                    eventData.PreferenceId);

                _logger.LogInformation($"Промокод {eventData.Code} успешно выдан клиентам с предпочтением {eventData.PreferenceId}");
            }
            catch (System.ArgumentException ex)
            {
                _logger.LogWarning(ex, $"Ошибка при выдаче промокода {eventData.Code}: {ex.Message}");
            }
        }
    }
} 