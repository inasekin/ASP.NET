using System;
using System.Threading.Tasks;
using Pcf.GivingToCustomer.Core.Domain;

namespace Pcf.GivingToCustomer.Core.Services
{
    /// <summary>
    /// Интерфейс сервиса для обработки промокодов
    /// </summary>
    public interface IPromoCodeService
    {
        /// <summary>
        /// Выдать промокод клиентам с указанным предпочтением
        /// </summary>
        /// <param name="promoCodeId">ID промокода</param>
        /// <param name="code">Код промокода</param>
        /// <param name="serviceInfo">Информация о сервисе</param>
        /// <param name="beginDate">Дата начала действия</param>
        /// <param name="endDate">Дата окончания действия</param>
        /// <param name="partnerId">ID партнера</param>
        /// <param name="preferenceId">ID предпочтения</param>
        /// <returns>Созданный промокод</returns>
        Task<PromoCode> GivePromoCodeToCustomersAsync(
            Guid promoCodeId,
            string code,
            string serviceInfo,
            DateTime beginDate,
            DateTime endDate,
            Guid partnerId,
            Guid preferenceId);
    }
} 