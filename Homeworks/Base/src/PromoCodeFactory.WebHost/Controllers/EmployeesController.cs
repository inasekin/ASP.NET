using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PromoCodeFactory.WebHost.Models;
using PromoCodeFactory.WebHost.Services;

namespace PromoCodeFactory.WebHost.Controllers
{
    /// <summary>
    /// Контроллер для управления сотрудниками.
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    public class EmployeesController : ControllerBase
    {
        private readonly EmployeeService _employeeService;

        /// <summary>
        /// Конструктор контроллера сотрудников.
        /// </summary>
        /// <param name="employeeService">Сервис для работы с сотрудниками.</param>
        public EmployeesController(EmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        /// <summary>
        /// Получить список всех сотрудников.
        /// </summary>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        /// <returns>Список краткой информации о сотрудниках.</returns>
        [HttpGet]
        public async Task<ActionResult<List<EmployeeShortResponse>>> GetEmployeesAsync(CancellationToken cancellationToken)
        {
            var employees = await _employeeService.GetAllEmployeesAsync(cancellationToken);
            return Ok(employees);
        }

        /// <summary>
        /// Получить информацию о сотруднике по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор сотрудника.</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        /// <returns>Полная информация о сотруднике.</returns>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<EmployeeResponse>> GetEmployeeByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id, cancellationToken);
            if (employee == null)
                return NotFound();

            return Ok(employee);
        }

        /// <summary>
        /// Создать нового сотрудника.
        /// </summary>
        /// <param name="request">Данные для создания сотрудника.</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        /// <returns>Информация о созданном сотруднике.</returns>
        [HttpPost]
        public async Task<ActionResult<EmployeeResponse>> CreateEmployeeAsync(EmployeeCreateRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var employee = await _employeeService.CreateEmployeeAsync(request, cancellationToken);
                return Ok(employee);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Удалить сотрудника по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор сотрудника.</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        /// <returns>Статус операции удаления.</returns>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteEmployeeAsync(Guid id, CancellationToken cancellationToken)
        {
            var result = await _employeeService.DeleteEmployeeAsync(id, cancellationToken);
            if (!result)
                return NotFound(new { Error = "Сотрудник не найден." });

            return NoContent();
        }

        /// <summary>
        /// Обновить данные сотрудника.
        /// </summary>
        /// <param name="id">Идентификатор сотрудника.</param>
        /// <param name="request">Данные для обновления сотрудника.</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        /// <returns>Статус операции обновления.</returns>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateEmployeeAsync(Guid id, EmployeeUpdateRequest request, CancellationToken cancellationToken)
        {
            try
            {
                await _employeeService.UpdateEmployeeAsync(id, request, cancellationToken);
                return NoContent();
            }
            catch (KeyNotFoundException ex) // Если сотрудник не найден
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (ArgumentException ex) // Если переданы некорректные данные
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
    }
}
