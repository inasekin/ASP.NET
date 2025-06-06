using System;

namespace Pcf.ReceivingFromPartner.Core.Models
{
    /// <summary>
    /// Запрос на получение промокода от партнера
    /// </summary>
    public class ReceivingPromoCodeRequest
    {
        public string ServiceInfo { get; set; } = string.Empty;

        public string PromoCode { get; set; } = string.Empty;

        public Guid PreferenceId { get; set; }

        public Guid? PartnerManagerId { get; set; }
    }
} 