using System;
using System.Linq;
using System.Threading.Tasks;
using Pcf.ReceivingFromPartner.Core.Abstractions.Repositories;
using Pcf.ReceivingFromPartner.Core.Domain;
using Pcf.ReceivingFromPartner.Core.Models;

namespace Pcf.ReceivingFromPartner.Core.Services
{
    /// <summary>
    /// Сервис для обработки промокодов
    /// </summary>
    public class PromoCodeService : IPromoCodeService
    {
        private readonly IRepository<Partner> _partnersRepository;
        private readonly IRepository<Preference> _preferencesRepository;

        public PromoCodeService(
            IRepository<Partner> partnersRepository,
            IRepository<Preference> preferencesRepository)
        {
            _partnersRepository = partnersRepository;
            _preferencesRepository = preferencesRepository;
        }

        public async Task<PromoCode> ProcessPromoCodeFromPartnerAsync(Guid partnerId, ReceivingPromoCodeRequest request)
        {
            var partner = await _partnersRepository.GetByIdAsync(partnerId);

            if (partner == null)
            {
                throw new ArgumentException("Партнер не найден", nameof(partnerId));
            }

            var activeLimit = partner.PartnerLimits.FirstOrDefault(x
                => !x.CancelDate.HasValue && x.EndDate > DateTime.Now);

            if (activeLimit == null)
            {
                throw new InvalidOperationException("Нет доступного лимита на предоставление промокодов");
            }

            if (partner.NumberIssuedPromoCodes + 1 > activeLimit.Limit)
            {
                throw new InvalidOperationException("Лимит на выдачу промокодов превышен");
            }

            if (partner.PromoCodes.Any(x => x.Code == request.PromoCode))
            {
                throw new InvalidOperationException("Данный промокод уже был выдан ранее");
            }

            // Получаем предпочтение по ID
            var preference = await _preferencesRepository.GetByIdAsync(request.PreferenceId);

            if (preference == null)
            {
                throw new ArgumentException("Предпочтение не найдено", nameof(request.PreferenceId));
            }

            // Создаем промокод
            var promoCode = new PromoCode
            {
                Id = Guid.NewGuid(),
                PartnerId = partner.Id,
                Partner = partner,
                Code = request.PromoCode,
                ServiceInfo = request.ServiceInfo,
                BeginDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(30),
                Preference = preference,
                PreferenceId = preference.Id,
                PartnerManagerId = request.PartnerManagerId
            };

            partner.PromoCodes.Add(promoCode);
            partner.NumberIssuedPromoCodes++;

            await _partnersRepository.UpdateAsync(partner);

            return promoCode;
        }
    }
} 