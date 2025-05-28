using System;

namespace Pcf.Common.Events.Events
{
    /// <summary>
    /// Событие о получении промокода от партнера
    /// </summary>
    public class PromoCodeReceivedEvent
    {
        public Guid PromoCodeId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string ServiceInfo { get; set; } = string.Empty;
        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }
        public Guid PartnerId { get; set; }
        public Guid PreferenceId { get; set; }
        public Guid? PartnerManagerId { get; set; }
        public DateTime EventTimestamp { get; set; } = DateTime.UtcNow;
    }
} 