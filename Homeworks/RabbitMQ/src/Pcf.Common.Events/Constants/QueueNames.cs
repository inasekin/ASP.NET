namespace Pcf.Common.Events.Constants
{
    /// <summary>
    /// Константы имен очередей RabbitMQ
    /// </summary>
    public static class QueueNames
    {
        public const string PromoCodeReceived = "promocode.received";
        public const string PromoCodeReceivedAdministration = "promocode.received.administration";
        public const string PromoCodeReceivedGivingToCustomer = "promocode.received.giving-to-customer";
    }

    /// <summary>
    /// Константы имен exchange
    /// </summary>
    public static class ExchangeNames
    {
        public const string PromoCodeEvents = "promocode.events";
    }

    /// <summary>
    /// Константы routing keys
    /// </summary>
    public static class RoutingKeys
    {
        public const string PromoCodeReceived = "promocode.received";
    }
} 