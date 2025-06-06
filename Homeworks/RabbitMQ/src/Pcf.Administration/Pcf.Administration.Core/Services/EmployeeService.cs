using System;
using System.Threading.Tasks;
using Pcf.Administration.Core.Abstractions.Repositories;
using Pcf.Administration.Core.Domain.Administration;

namespace Pcf.Administration.Core.Services
{
    /// <summary>
    /// Сервис для работы с сотрудниками
    /// </summary>
    public class EmployeeService : IEmployeeService
    {
        private readonly IRepository<Employee> _employeeRepository;

        public EmployeeService(IRepository<Employee> employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        public async Task UpdateAppliedPromocodesCountAsync(Guid employeeId)
        {
            var employee = await _employeeRepository.GetByIdAsync(employeeId);

            if (employee == null)
            {
                throw new ArgumentException($"Сотрудник с ID {employeeId} не найден", nameof(employeeId));
            }

            employee.AppliedPromocodesCount++;
            await _employeeRepository.UpdateAsync(employee);
        }
    }
} 