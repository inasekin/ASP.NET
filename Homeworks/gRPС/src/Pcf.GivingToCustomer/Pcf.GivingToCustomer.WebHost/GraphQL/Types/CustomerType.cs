using System;
using System.Collections.Generic;
using System.Linq;
using HotChocolate;
using Pcf.GivingToCustomer.Core.Domain;

namespace Pcf.GivingToCustomer.WebHost.GraphQL.Types
{
    /// <summary>
    /// GraphQL тип для клиента
    /// </summary>
    public class CustomerType
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
        public List<PreferenceType> Preferences { get; set; } = new();
        public List<PromoCodeType> PromoCodes { get; set; } = new();

        public static CustomerType FromDomain(Customer customer)
        {
            return new CustomerType
            {
                Id = customer.Id,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                Preferences = customer.Preferences?.Select(PreferenceType.FromDomain).ToList() ?? new List<PreferenceType>(),
                PromoCodes = customer.PromoCodes?.Select(PromoCodeType.FromDomain).ToList() ?? new List<PromoCodeType>()
            };
        }
    }

    /// <summary>
    /// GraphQL тип для краткой информации о клиенте
    /// </summary>
    public class CustomerShortType
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";

        public static CustomerShortType FromDomain(Customer customer)
        {
            return new CustomerShortType
            {
                Id = customer.Id,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email
            };
        }
    }

    /// <summary>
    /// GraphQL тип для предпочтений
    /// </summary>
    public class PreferenceType
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public static PreferenceType FromDomain(CustomerPreference customerPreference)
        {
            return new PreferenceType
            {
                Id = customerPreference.PreferenceId,
                Name = customerPreference.Preference.Name
            };
        }

        public static PreferenceType FromPreference(Preference preference)
        {
            return new PreferenceType
            {
                Id = preference.Id,
                Name = preference.Name
            };
        }
    }

    /// <summary>
    /// GraphQL тип для промокодов
    /// </summary>
    public class PromoCodeType
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string BeginDate { get; set; } = string.Empty;
        public string EndDate { get; set; } = string.Empty;
        public Guid PartnerId { get; set; }
        public string ServiceInfo { get; set; } = string.Empty;

        public static PromoCodeType FromDomain(PromoCodeCustomer promoCodeCustomer)
        {
            return new PromoCodeType
            {
                Id = promoCodeCustomer.PromoCodeId,
                Code = promoCodeCustomer.PromoCode.Code,
                BeginDate = promoCodeCustomer.PromoCode.BeginDate.ToString("yyyy-MM-dd"),
                EndDate = promoCodeCustomer.PromoCode.EndDate.ToString("yyyy-MM-dd"),
                PartnerId = promoCodeCustomer.PromoCode.PartnerId,
                ServiceInfo = promoCodeCustomer.PromoCode.ServiceInfo
            };
        }
    }
} 