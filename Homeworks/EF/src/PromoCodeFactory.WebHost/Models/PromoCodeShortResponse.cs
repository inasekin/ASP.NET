using System;

namespace PromoCodeFactory.WebHost.Models
{
    public class PromoCodeShortResponse
    {
        public Guid Id { get; set; }

        public string Code { get; set; }

        public string ServiceInfo { get; set; }

        public DateTime BeginDate { get; set; }

        public DateTime EndDate { get; set; }
    }
}