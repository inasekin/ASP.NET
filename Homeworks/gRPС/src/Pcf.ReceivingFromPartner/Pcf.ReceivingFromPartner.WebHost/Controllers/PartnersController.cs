using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Pcf.ReceivingFromPartner.Core.Abstractions.Repositories;
using Pcf.ReceivingFromPartner.Core.Domain;
using Pcf.ReceivingFromPartner.Core.Abstractions.Gateways;
using Pcf.ReceivingFromPartner.Core.Services;
using Pcf.ReceivingFromPartner.WebHost.Models;
using Pcf.ReceivingFromPartner.WebHost.Mappers;
using Pcf.Common.Events.Abstractions;
using Pcf.Common.Events.Events;
using Pcf.Common.Events.Constants;

namespace Pcf.ReceivingFromPartner.WebHost.Controllers
{
    /// <summary>
    /// Партнеры
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    public class PartnersController
        : ControllerBase
    {
        private readonly IRepository<Partner> _partnersRepository;
        private readonly IRepository<Preference> _preferencesRepository;
        private readonly INotificationGateway _notificationGateway;
        private readonly IPromoCodeService _promoCodeService;
        private readonly IEventPublisher _eventPublisher;

        public PartnersController(IRepository<Partner> partnersRepository,
            IRepository<Preference> preferencesRepository,
            INotificationGateway notificationGateway,
            IPromoCodeService promoCodeService,
            IEventPublisher eventPublisher)
        {
            _partnersRepository = partnersRepository;
            _preferencesRepository = preferencesRepository;
            _notificationGateway = notificationGateway;
            _promoCodeService = promoCodeService;
            _eventPublisher = eventPublisher;
        }

        /// <summary>
        /// Получить список партнеров
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<PartnerResponse>>> GetPartnersAsync()
        {
            var partners = await _partnersRepository.GetAllAsync();

            var response = partners.Select(x => new PartnerResponse()
            {
                Id = x.Id,
                Name = x.Name,
                NumberIssuedPromoCodes = x.NumberIssuedPromoCodes,
                IsActive = x.IsActive,
                PartnerLimits = x.PartnerLimits.Select(y => new PartnerPromoCodeLimitResponse()
                {
                    Id = y.Id,
                    PartnerId = y.PartnerId,
                    CreateDate = y.CreateDate.ToString("dd.MM.yyyy hh:mm:ss"),
                    EndDate = y.EndDate.ToString("dd.MM.yyyy hh:mm:ss"),
                    CancelDate = y.CancelDate?.ToString("dd.MM.yyyy hh:mm:ss"),
                    Limit = y.Limit
                }).ToList()
            }).ToList();

            return Ok(response);
        }

        /// <summary>
        /// Получить партнера по Id
        /// </summary>
        /// <param name="id">Id партнера, например: <example>20d2d612-db93-4ed5-86b1-ff2413bca655</example></param>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<List<PartnerResponse>>> GetPartnersAsync(Guid id)
        {
            var partner = await _partnersRepository.GetByIdAsync(id);

            if (partner == null)
                return NotFound();

            var response = new PartnerResponse()
            {
                Id = partner.Id,
                Name = partner.Name,
                NumberIssuedPromoCodes = partner.NumberIssuedPromoCodes,
                IsActive = partner.IsActive,
                PartnerLimits = partner.PartnerLimits.Select(y => new PartnerPromoCodeLimitResponse()
                {
                    Id = y.Id,
                    PartnerId = y.PartnerId,
                    CreateDate = y.CreateDate.ToString("dd.MM.yyyy hh:mm:ss"),
                    EndDate = y.EndDate.ToString("dd.MM.yyyy hh:mm:ss"),
                    CancelDate = y.CancelDate?.ToString("dd.MM.yyyy hh:mm:ss"),
                    Limit = y.Limit
                }).ToList()
            };

            return Ok(response);
        }

        /// <summary>
        /// Установить лимит на промокоды для партнера
        /// </summary>
        /// <param name="id">Id партнера, например: <example>20d2d612-db93-4ed5-86b1-ff2413bca655</example></param>
        /// <param name="request">Данные запроса</param>
        [HttpPost("{id:guid}/limits")]
        public async Task<IActionResult> SetPartnerPromoCodeLimitAsync(Guid id, SetPartnerPromoCodeLimitRequest request)
        {
            var partner = await _partnersRepository.GetByIdAsync(id);

            if (partner == null)
                return NotFound();

            //Если партнер заблокирован, то нужно выдать исключение
            if (!partner.IsActive)
                return BadRequest("Данный партнер не активен");

            //Отключение лимита
            var activeLimit = partner.PartnerLimits.FirstOrDefault(x =>
                !x.CancelDate.HasValue);

            if (activeLimit != null)
            {
                activeLimit.CancelDate = DateTime.Now;
            }

            //Установка лимита
            var newLimit = new PartnerPromoCodeLimit()
            {
                Id = Guid.NewGuid(),
                PartnerId = partner.Id,
                Partner = partner,
                Limit = request.Limit,
                CreateDate = DateTime.Now,
                EndDate = request.EndDate
            };

            partner.PartnerLimits.Add(newLimit);

            await _partnersRepository.UpdateAsync(partner);

            //Отправляем уведомление
            await _notificationGateway
                .SendNotificationToPartnerAsync(partner.Id, "Вам установлен лимит на отправку промокодов...");

            return CreatedAtAction(nameof(GetPartnerLimitAsync), new { id = partner.Id, limitId = newLimit.Id }, null);
        }

        /// <summary>
        /// Получить лимит на промокоды для партнера
        /// </summary>
        /// <param name="id">Id партнера, например: <example>20d2d612-db93-4ed5-86b1-ff2413bca655</example></param>
        /// <param name="limitId">Id лимита партнера, например: <example>93f3a79d-e9f9-47e6-98bb-1f618db43230</example></param>
        [HttpGet("{id:guid}/limits/{limitId:guid}")]
        public async Task<ActionResult<PartnerPromoCodeLimit>> GetPartnerLimitAsync(Guid id, Guid limitId)
        {
            var partner = await _partnersRepository.GetByIdAsync(id);

            if (partner == null)
                return NotFound();

            var limit = partner.PartnerLimits
                .FirstOrDefault(x => x.Id == limitId);

            var response = new PartnerPromoCodeLimitResponse()
            {
                Id = limit.Id,
                PartnerId = limit.PartnerId,
                Limit = limit.Limit,
                CreateDate = limit.CreateDate.ToString("dd.MM.yyyy hh:mm:ss"),
                EndDate = limit.EndDate.ToString("dd.MM.yyyy hh:mm:ss"),
                CancelDate = limit.CancelDate?.ToString("dd.MM.yyyy hh:mm:ss"),
            };

            return Ok(response);
        }

        /// <summary>
        /// Отменить лимит на промокоды для партнера
        /// </summary>
        /// <param name="id">Id партнера, например: <example>0da65561-cf56-4942-bff2-22f50cf70d43</example></param>
        [HttpPost("{id:guid}/canceledLimits")]
        public async Task<IActionResult> CancelPartnerPromoCodeLimitAsync(Guid id)
        {
            var partner = await _partnersRepository.GetByIdAsync(id);

            if (partner == null)
                return NotFound();

            //Если партнер заблокирован, то нужно выдать исключение
            if (!partner.IsActive)
                return BadRequest("Данный партнер не активен");

            //Отключение лимита
            var activeLimit = partner.PartnerLimits.FirstOrDefault(x =>
                !x.CancelDate.HasValue);

            if (activeLimit != null)
            {
                activeLimit.CancelDate = DateTime.Now;
            }

            await _partnersRepository.UpdateAsync(partner);

            //Отправляем уведомление
            await _notificationGateway
                .SendNotificationToPartnerAsync(partner.Id, "Ваш лимит на отправку промокодов отменен...");

            return NoContent();
        }

        /// <summary>
        /// Получить промокод партнера по id
        /// </summary>
        /// <returns></returns>
        [HttpGet("{id:guid}/promocodes")]
        public async Task<IActionResult> GetPartnerPromoCodesAsync(Guid id)
        {
            var partner = await _partnersRepository.GetByIdAsync(id);

            if (partner == null)
            {
                return NotFound("Партнер не найден");
            }

            var response = partner.PromoCodes
                .Select(x => new PromoCodeShortResponse()
                {
                    Id = x.Id,
                    Code = x.Code,
                    BeginDate = x.BeginDate.ToString("yyyy-MM-dd"),
                    EndDate = x.EndDate.ToString("yyyy-MM-dd"),
                    PartnerName = x.Partner.Name,
                    PartnerId = x.PartnerId,
                    ServiceInfo = x.ServiceInfo
                }).ToList();

            return Ok(response);
        }

        /// <summary>
        /// Получить промокод партнера по id
        /// </summary>
        /// <returns></returns>
        [HttpGet("{id:guid}/promocodes/{promoCodeId:guid}")]
        public async Task<IActionResult> GetPartnerPromoCodeAsync(Guid id, Guid promoCodeId)
        {
            var partner = await _partnersRepository.GetByIdAsync(id);

            if (partner == null)
            {
                return NotFound("Партнер не найден");
            }

            var promoCode = partner.PromoCodes.FirstOrDefault(x => x.Id == promoCodeId);

            if (promoCode == null)
            {
                return NotFound("Партнер не найден");
            }

            var response = new PromoCodeShortResponse()
            {
                Id = promoCode.Id,
                Code = promoCode.Code,
                BeginDate = promoCode.BeginDate.ToString("yyyy-MM-dd"),
                EndDate = promoCode.EndDate.ToString("yyyy-MM-dd"),
                PartnerName = promoCode.Partner.Name,
                PartnerId = promoCode.PartnerId,
                ServiceInfo = promoCode.ServiceInfo
            };

            return Ok(response);
        }

        /// <summary>
        /// Создать промокод от партнера 
        /// </summary>
        /// <param name="id">Id партнера, например: <example>20d2d612-db93-4ed5-86b1-ff2413bca655</example></param>
        /// <param name="request">Данные запроса/example></param>
        /// <returns></returns>
        [HttpPost("{id:guid}/promocodes")]
        public async Task<IActionResult> ReceivePromoCodeFromPartnerWithPreferenceAsync(Guid id,
            ReceivingPromoCodeRequest request)
        {
            try
            {
                // Маппим модель из WebHost в Core
                var coreRequest = new Core.Models.ReceivingPromoCodeRequest
                {
                    ServiceInfo = request.ServiceInfo,
                    PromoCode = request.PromoCode,
                    PreferenceId = request.PreferenceId,
                    PartnerManagerId = request.PartnerManagerId
                };

                // Используем сервис для обработки промокода
                var promoCode = await _promoCodeService.ProcessPromoCodeFromPartnerAsync(id, coreRequest);

                // Публикуем событие в RabbitMQ вместо синхронных HTTP вызовов
                var promoCodeEvent = new PromoCodeReceivedEvent
                {
                    PromoCodeId = promoCode.Id,
                    Code = promoCode.Code,
                    ServiceInfo = promoCode.ServiceInfo,
                    BeginDate = promoCode.BeginDate,
                    EndDate = promoCode.EndDate,
                    PartnerId = promoCode.PartnerId,
                    PreferenceId = promoCode.PreferenceId,
                    PartnerManagerId = promoCode.PartnerManagerId,
                    EventTimestamp = DateTime.UtcNow
                };

                await _eventPublisher.PublishAsync(promoCodeEvent, RoutingKeys.PromoCodeReceived);

                return CreatedAtAction(nameof(GetPartnerPromoCodeAsync),
                    new { id = promoCode.PartnerId, promoCodeId = promoCode.Id }, null);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}