using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Types;
using Pcf.GivingToCustomer.Core.Abstractions.Repositories;
using Pcf.GivingToCustomer.Core.Domain;
using Pcf.GivingToCustomer.WebHost.GraphQL.Types;

namespace Pcf.GivingToCustomer.WebHost.GraphQL.Queries
{
    /// <summary>
    /// GraphQL запросы для работы с клиентами
    /// </summary>
    [ExtendObjectType("Query")]
    public class CustomerQueries
    {
        /// <summary>
        /// Получить список всех клиентов
        /// </summary>
        [GraphQLDescription("Получить список всех клиентов")]
        public async Task<List<CustomerShortType>> GetCustomersAsync(
            [Service] IRepository<Customer> customerRepository)
        {
            var customers = await customerRepository.GetAllAsync();
            return customers.Select(CustomerShortType.FromDomain).ToList();
        }

        /// <summary>
        /// Получить клиента по ID
        /// </summary>
        [GraphQLDescription("Получить детальную информацию о клиенте по ID")]
        public async Task<CustomerType> GetCustomerAsync(
            [GraphQLDescription("ID клиента")] Guid id,
            [Service] IRepository<Customer> customerRepository)
        {
            var customer = await customerRepository.GetByIdAsync(id);
            return customer != null ? CustomerType.FromDomain(customer) : null;
        }

        /// <summary>
        /// Поиск клиентов по email
        /// </summary>
        [GraphQLDescription("Поиск клиентов по email")]
        public async Task<List<CustomerShortType>> SearchCustomersByEmailAsync(
            [GraphQLDescription("Email для поиска")] string email,
            [Service] IRepository<Customer> customerRepository)
        {
            var customers = await customerRepository.GetWhere(c => c.Email.Contains(email));
            return customers.Select(CustomerShortType.FromDomain).ToList();
        }

        /// <summary>
        /// Получить клиентов по предпочтению
        /// </summary>
        [GraphQLDescription("Получить клиентов с определенным предпочтением")]
        public async Task<List<CustomerShortType>> GetCustomersByPreferenceAsync(
            [GraphQLDescription("ID предпочтения")] Guid preferenceId,
            [Service] IRepository<Customer> customerRepository)
        {
            var customers = await customerRepository.GetWhere(c => 
                c.Preferences.Any(p => p.PreferenceId == preferenceId));
            return customers.Select(CustomerShortType.FromDomain).ToList();
        }

        /// <summary>
        /// Получить все предпочтения
        /// </summary>
        [GraphQLDescription("Получить список всех доступных предпочтений")]
        public async Task<List<PreferenceType>> GetPreferencesAsync(
            [Service] IRepository<Preference> preferenceRepository)
        {
            var preferences = await preferenceRepository.GetAllAsync();
            return preferences.Select(PreferenceType.FromPreference).ToList();
        }
    }
} 