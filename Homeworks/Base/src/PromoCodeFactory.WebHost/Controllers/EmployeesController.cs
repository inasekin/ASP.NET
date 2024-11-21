using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<EmployeesController> _logger;

        /// <summary>
        /// Конструктор контроллера сотрудников.
        /// </summary>
        /// <param name="employeeService">Сервис для работы с сотрудниками.</param>
        /// <param name="logger">Логгер для записи событий.</param>
        public EmployeesController(EmployeeService employeeService, ILogger<EmployeesController> logger)
        {
            _employeeService = employeeService;
            _logger = logger;
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
            {
                _logger.LogWarning("Сотрудник с идентификатором {EmployeeId} не найден.", id);
                return NotFound(new { Message = "Сотрудник не найден." });
            }

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
                return CreatedAtAction(nameof(GetEmployeeByIdAsync), new { id = employee.Id }, employee);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Ошибка при создании сотрудника: {ErrorMessage}", ex.Message);
                return BadRequest(new { Message = "Некорректные данные запроса." });
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
            {
                _logger.LogWarning("Попытка удаления сотрудника с идентификатором {EmployeeId}, который не найден.", id);
                return NotFound(new { Message = "Сотрудник не найден." });
            }

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
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Сотрудник с идентификатором {EmployeeId} не найден для обновления.", id);
                return NotFound(new { Message = "Сотрудник не найден." });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Некорректные данные для обновления сотрудника с идентификатором {EmployeeId}: {ErrorMessage}", id, ex.Message);
                return BadRequest(new { Message = "Некорректные данные запроса." });
            }
        }
    }
}