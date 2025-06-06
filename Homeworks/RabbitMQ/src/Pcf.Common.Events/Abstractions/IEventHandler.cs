using System.Threading.Tasks;

namespace Pcf.Common.Events.Abstractions
{
    /// <summary>
    /// Интерфейс для обработки событий
    /// </summary>
    /// <typeparam name="T">Тип события</typeparam>
    public interface IEventHandler<in T> where T : class
    {
        /// <summary>
        /// Обработать событие
        /// </summary>
        /// <param name="eventData">Данные события</param>
        /// <returns></returns>
        Task HandleAsync(T eventData);
    }
} 