using System;
using System.Threading.Tasks;

namespace Pcf.Administration.Core.Services
{
    /// <summary>
    /// Интерфейс сервиса для работы с сотрудниками
    /// </summary>
    public interface IEmployeeService
    {
        /// <summary>
        /// Обновить количество выданных промокодов сотрудника
        /// </summary>
        /// <param name="employeeId">ID сотрудника</param>
        /// <returns></returns>
        Task UpdateAppliedPromocodesCountAsync(Guid employeeId);
    }
} 