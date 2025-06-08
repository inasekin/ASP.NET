using System;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using Pcf.GivingToCustomer.Core.Abstractions.Repositories;
using Pcf.GivingToCustomer.Core.Domain;
using Pcf.GivingToCustomer.WebHost.GraphQL.Inputs;
using Pcf.GivingToCustomer.WebHost.GraphQL.Types;
using Pcf.GivingToCustomer.WebHost.Mappers;
using Pcf.GivingToCustomer.WebHost.Models;

namespace Pcf.GivingToCustomer.WebHost.GraphQL.Mutations
{
    /// <summary>
    /// GraphQL мутации для работы с клиентами
    /// </summary>
    [ExtendObjectType("Mutation")]
    public class CustomerMutations
    {
        /// <summary>
        /// Создать нового клиента
        /// </summary>
        [GraphQLDescription("Создать нового клиента")]
        public async Task<CustomerType> CreateCustomerAsync(
            [GraphQLDescription("Данные для создания клиента")] CreateCustomerInput input,
            [Service] IRepository<Customer> customerRepository,
            [Service] IRepository<Preference> preferenceRepository,
            [Service] ILogger<CustomerMutations> logger)
        {
            try
            {
                // Получаем предпочтения из базы данных
                var preferences = await preferenceRepository.GetRangeByIdsAsync(input.PreferenceIds);

                // Создаем запрос для маппера
                var createRequest = new CreateOrEditCustomerRequest
                {
                    FirstName = input.FirstName,
                    LastName = input.LastName,
                    Email = input.Email,
                    PreferenceIds = input.PreferenceIds
                };

                // Маппим в доменную модель
                var customer = CustomerMapper.MapFromModel(createRequest, preferences);

                // Сохраняем в базе данных
                await customerRepository.AddAsync(customer);

                logger.LogInformation("Создан новый клиент через GraphQL: {CustomerId}", customer.Id);

                return CustomerType.FromDomain(customer);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при создании клиента через GraphQL");
                throw new GraphQLException("Произошла ошибка при создании клиента");
            }
        }

        /// <summary>
        /// Обновить существующего клиента
        /// </summary>
        [GraphQLDescription("Обновить данные существующего клиента")]
        public async Task<CustomerType> UpdateCustomerAsync(
            [GraphQLDescription("Данные для обновления клиента")] UpdateCustomerInput input,
            [Service] IRepository<Customer> customerRepository,
            [Service] IRepository<Preference> preferenceRepository,
            [Service] ILogger<CustomerMutations> logger)
        {
            try
            {
                // Ищем существующего клиента
                var existingCustomer = await customerRepository.GetByIdAsync(input.Id);
                if (existingCustomer == null)
                {
                    throw new GraphQLException($"Клиент с ID {input.Id} не найден");
                }

                // Получаем предпочтения из базы данных
                var preferences = await preferenceRepository.GetRangeByIdsAsync(input.PreferenceIds);

                // Создаем запрос для маппера
                var updateRequest = new CreateOrEditCustomerRequest
                {
                    FirstName = input.FirstName,
                    LastName = input.LastName,
                    Email = input.Email,
                    PreferenceIds = input.PreferenceIds
                };

                // Обновляем существующего клиента
                CustomerMapper.MapFromModel(updateRequest, preferences, existingCustomer);

                // Сохраняем изменения
                await customerRepository.UpdateAsync(existingCustomer);

                logger.LogInformation("Обновлен клиент через GraphQL: {CustomerId}", existingCustomer.Id);

                return CustomerType.FromDomain(existingCustomer);
            }
            catch (GraphQLException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при обновлении клиента {CustomerId} через GraphQL", input.Id);
                throw new GraphQLException("Произошла ошибка при обновлении клиента");
            }
        }

        /// <summary>
        /// Удалить клиента
        /// </summary>
        [GraphQLDescription("Удалить клиента")]
        public async Task<bool> DeleteCustomerAsync(
            [GraphQLDescription("Данные для удаления клиента")] DeleteCustomerInput input,
            [Service] IRepository<Customer> customerRepository,
            [Service] ILogger<CustomerMutations> logger)
        {
            try
            {
                // Ищем существующего клиента
                var existingCustomer = await customerRepository.GetByIdAsync(input.Id);
                if (existingCustomer == null)
                {
                    throw new GraphQLException($"Клиент с ID {input.Id} не найден");
                }

                // Удаляем клиента
                await customerRepository.DeleteAsync(existingCustomer);

                logger.LogInformation("Удален клиент через GraphQL: {CustomerId}", input.Id);

                return true;
            }
            catch (GraphQLException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при удалении клиента {CustomerId} через GraphQL", input.Id);
                throw new GraphQLException("Произошла ошибка при удалении клиента");
            }
        }
    }
} 