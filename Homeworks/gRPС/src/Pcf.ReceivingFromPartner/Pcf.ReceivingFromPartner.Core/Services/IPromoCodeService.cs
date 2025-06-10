using System;
using System.Threading.Tasks;
using Pcf.ReceivingFromPartner.Core.Domain;
using Pcf.ReceivingFromPartner.Core.Models;

namespace Pcf.ReceivingFromPartner.Core.Services
{
    /// <summary>
    /// Интерфейс сервиса для обработки промокодов
    /// </summary>
    public interface IPromoCodeService
    {
        /// <summary>
        /// Обработать получение промокода от партнера
        /// </summary>
        /// <param name="partnerId">ID партнера</param>
        /// <param name="request">Запрос на создание промокода</param>
        /// <returns>Созданный промокод</returns>
        Task<PromoCode> ProcessPromoCodeFromPartnerAsync(Guid partnerId, ReceivingPromoCodeRequest request);
    }
} 