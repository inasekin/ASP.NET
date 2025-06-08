using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pcf.GivingToCustomer.Core.Abstractions.Repositories;
using Pcf.GivingToCustomer.Core.Domain;

namespace Pcf.GivingToCustomer.Core.Services
{
    /// <summary>
    /// Сервис для обработки промокодов
    /// </summary>
    public class PromoCodeService : IPromoCodeService
    {
        private readonly IRepository<PromoCode> _promoCodesRepository;
        private readonly IRepository<Preference> _preferencesRepository;
        private readonly IRepository<Customer> _customersRepository;

        public PromoCodeService(
            IRepository<PromoCode> promoCodesRepository,
            IRepository<Preference> preferencesRepository,
            IRepository<Customer> customersRepository)
        {
            _promoCodesRepository = promoCodesRepository;
            _preferencesRepository = preferencesRepository;
            _customersRepository = customersRepository;
        }

        public async Task<PromoCode> GivePromoCodeToCustomersAsync(
            Guid promoCodeId,
            string code,
            string serviceInfo,
            DateTime beginDate,
            DateTime endDate,
            Guid partnerId,
            Guid preferenceId)
        {
            // Получаем предпочтение по ID
            var preference = await _preferencesRepository.GetByIdAsync(preferenceId);

            if (preference == null)
            {
                throw new ArgumentException($"Предпочтение с ID {preferenceId} не найдено", nameof(preferenceId));
            }

            // Получаем клиентов с этим предпочтением
            var customers = await _customersRepository
                .GetWhere(d => d.Preferences.Any(x => x.Preference.Id == preference.Id));

            // Создаем промокод
            var promoCode = new PromoCode
            {
                Id = promoCodeId,
                PartnerId = partnerId,
                Code = code,
                ServiceInfo = serviceInfo,
                BeginDate = beginDate,
                EndDate = endDate,
                Preference = preference,
                PreferenceId = preference.Id,
                Customers = new List<PromoCodeCustomer>()
            };

            // Привязываем промокод к клиентам
            foreach (var customer in customers)
            {
                promoCode.Customers.Add(new PromoCodeCustomer
                {
                    CustomerId = customer.Id,
                    Customer = customer,
                    PromoCodeId = promoCode.Id,
                    PromoCode = promoCode
                });
            }

            await _promoCodesRepository.AddAsync(promoCode);

            return promoCode;
        }
    }
} 