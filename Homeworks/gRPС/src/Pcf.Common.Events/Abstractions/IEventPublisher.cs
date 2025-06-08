using System.Threading.Tasks;

namespace Pcf.Common.Events.Abstractions
{
    /// <summary>
    /// Интерфейс для публикации событий
    /// </summary>
    public interface IEventPublisher
    {
        /// <summary>
        /// Опубликовать событие
        /// </summary>
        /// <typeparam name="T">Тип события</typeparam>
        /// <param name="eventData">Данные события</param>
        /// <param name="routingKey">Routing key</param>
        /// <returns></returns>
        Task PublishAsync<T>(T eventData, string routingKey) where T : class;
    }
} 