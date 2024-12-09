using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.DataAccess.Data;
using PromoCodeFactory.WebHost.Models;

namespace PromoCodeFactory.WebHost.Controllers
{
    /// <summary>
    /// Промокоды
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    public class PromocodesController(
        IRepository<PromoCode> promoCodeRepository,
        IRepository<Customer> customerRepository,
        DataContext dbContext)
        : ControllerBase
    {
        /// <summary>
        /// Получить промокод по Id
        /// </summary>
        /// <param name="id">Идентификатор промокода</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<PromoCodeShortResponse>> GetPromocodeByIdAsync(Guid id)
        {
            var promoCode = await promoCodeRepository.GetByIdAsync(id);
            if (promoCode == null)
            {
                return NotFound($"PromoCode с Id {id} не найден.");
            }

            var response = new PromoCodeShortResponse
            {
                Id = promoCode.Id,
                Code = promoCode.Code,
                ServiceInfo = promoCode.ServiceInfo,
                BeginDate = promoCode.BeginDate,
                EndDate = promoCode.EndDate
            };

            return Ok(response);
        }

        /// <summary>
        /// Получить все промокоды
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<List<PromoCodeShortResponse>>> GetPromocodesAsync()
        {
            var promoCodes = await promoCodeRepository.GetAllAsync();

            var response = promoCodes.Select(pc => new PromoCodeShortResponse
            {
                Id = pc.Id,
                Code = pc.Code,
                ServiceInfo = pc.ServiceInfo,
                BeginDate = pc.BeginDate,
                EndDate = pc.EndDate
            }).ToList();

            return Ok(response);
        }

        /// <summary>
        /// Создать промокод и выдать его клиентам с указанным предпочтением
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> GivePromoCodesToCustomersWithPreferenceAsync(
            [FromBody] GivePromoCodeRequest request)
        {
            // Проверяем существование предпочтения
            var preference = await dbContext.Preferences.FirstOrDefaultAsync(p => p.Id == request.PreferenceId);
            if (preference == null)
            {
                return BadRequest($"Preference с Id {request.PreferenceId} не найден.");
            }

            // Проверяем существование менеджера
            var partnerManager = await dbContext.Employees.FirstOrDefaultAsync(e => e.Id == request.CustomerId);
            if (partnerManager == null)
            {
                return BadRequest($"Customer с Id {request.CustomerId} не найден.");
            }

            // Создаём промокод
            var promoCode = new PromoCode
            {
                Id = Guid.NewGuid(),
                Code = request.Code,
                ServiceInfo = request.ServiceInfo,
                BeginDate = request.BeginDate,
                EndDate = request.EndDate,
                PreferenceId = request.PreferenceId,
            };

            await promoCodeRepository.AddAsync(promoCode);

            // Обновляем данные клиентов
            var customersWithPreference = await customerRepository.GetAllAsync();
            customersWithPreference = customersWithPreference
                .Where(c => c.CustomerPreferences.Any(cp => cp.PreferenceId == request.PreferenceId))
                .ToList();

            foreach (var customer in customersWithPreference)
            {
                customer.PromoCodes.Add(promoCode);
                await customerRepository.UpdateAsync(customer);
            }

            // Возвращаем успешный результат
            var createdPromoCode = new PromoCodeShortResponse
            {
                Id = promoCode.Id,
                Code = promoCode.Code,
                ServiceInfo = promoCode.ServiceInfo,
                BeginDate = promoCode.BeginDate,
                EndDate = promoCode.EndDate
            };

            // Используем прямой URI, чтобы исключить ошибки маршрутизации
            var location = Url.Action(nameof(GetPromocodeByIdAsync), new { id = promoCode.Id });

            return location != null
                ? Created(location, createdPromoCode)
                : Created($"api/v1/promocodes/{promoCode.Id}", createdPromoCode);
        }
    }
}