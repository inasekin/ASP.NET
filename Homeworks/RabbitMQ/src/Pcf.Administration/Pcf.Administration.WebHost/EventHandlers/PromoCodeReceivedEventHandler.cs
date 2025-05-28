using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pcf.Administration.Core.Services;
using Pcf.Common.Events.Abstractions;
using Pcf.Common.Events.Events;

namespace Pcf.Administration.WebHost.EventHandlers
{
    /// <summary>
    /// Обработчик событий получения промокодов для администрирования
    /// </summary>
    public class PromoCodeReceivedEventHandler : IEventHandler<PromoCodeReceivedEvent>
    {
        private readonly IEmployeeService _employeeService;
        private readonly ILogger<PromoCodeReceivedEventHandler> _logger;

        public PromoCodeReceivedEventHandler(
            IEmployeeService employeeService,
            ILogger<PromoCodeReceivedEventHandler> logger)
        {
            _employeeService = employeeService;
            _logger = logger;
        }

        public async Task HandleAsync(PromoCodeReceivedEvent eventData)
        {
            _logger.LogInformation($"Обработка события получения промокода: {JsonConvert.SerializeObject(eventData)}");

            // Если указан менеджер партнера, обновляем количество выданных промокодов
            if (eventData.PartnerManagerId.HasValue)
            {
                try
                {
                    await _employeeService.UpdateAppliedPromocodesCountAsync(eventData.PartnerManagerId.Value);
                    _logger.LogInformation($"Обновлен счетчик промокодов для сотрудника {eventData.PartnerManagerId.Value}");
                }
                catch (System.ArgumentException ex)
                {
                    _logger.LogWarning(ex, $"Сотрудник с ID {eventData.PartnerManagerId.Value} не найден");
                }
            }
        }
    }
} 