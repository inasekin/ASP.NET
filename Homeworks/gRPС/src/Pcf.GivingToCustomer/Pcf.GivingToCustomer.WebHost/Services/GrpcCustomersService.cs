using System;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Pcf.GivingToCustomer.Core.Abstractions.Repositories;
using Pcf.GivingToCustomer.WebHost.Grpc;
using Pcf.GivingToCustomer.WebHost.Mappers;
using DomainCustomer = Pcf.GivingToCustomer.Core.Domain.Customer;
using DomainPreference = Pcf.GivingToCustomer.Core.Domain.Preference;

namespace Pcf.GivingToCustomer.WebHost.Services
{
    public class GrpcCustomersService : CustomersService.CustomersServiceBase
    {
        private readonly IRepository<DomainCustomer> _customerRepository;
        private readonly IRepository<DomainPreference> _preferenceRepository;
        private readonly ILogger<GrpcCustomersService> _logger;

        public GrpcCustomersService(
            IRepository<DomainCustomer> customerRepository,
            IRepository<DomainPreference> preferenceRepository,
            ILogger<GrpcCustomersService> logger)
        {
            _customerRepository = customerRepository;
            _preferenceRepository = preferenceRepository;
            _logger = logger;
        }

        public override async Task<GetCustomersResponse> GetCustomers(GetCustomersRequest request, ServerCallContext context)
        {
            try
            {
                var customers = await _customerRepository.GetAllAsync();
                
                var response = new GetCustomersResponse();
                response.Customers.AddRange(customers.Select(c => new CustomerShort
                {
                    Id = c.Id.ToString(),
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    Email = c.Email
                }));

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка клиентов через gRPC");
                throw new RpcException(new Status(StatusCode.Internal, "Внутренняя ошибка сервера"));
            }
        }

        public override async Task<GetCustomerResponse> GetCustomer(GetCustomerRequest request, ServerCallContext context)
        {
            try
            {
                if (!Guid.TryParse(request.Id, out var customerId))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Некорректный ID клиента"));
                }

                var customer = await _customerRepository.GetByIdAsync(customerId);
                
                if (customer == null)
                {
                    throw new RpcException(new Status(StatusCode.NotFound, "Клиент не найден"));
                }

                var grpcCustomer = new Customer
                {
                    Id = customer.Id.ToString(),
                    FirstName = customer.FirstName,
                    LastName = customer.LastName,
                    Email = customer.Email
                };

                if (customer.Preferences != null)
                {
                    grpcCustomer.Preferences.AddRange(customer.Preferences.Select(p => new Preference
                    {
                        Id = p.PreferenceId.ToString(),
                        Name = p.Preference.Name
                    }));
                }

                if (customer.PromoCodes != null)
                {
                    grpcCustomer.PromoCodes.AddRange(customer.PromoCodes.Select(pc => new PromoCode
                    {
                        Id = pc.PromoCodeId.ToString(),
                        Code = pc.PromoCode.Code,
                        BeginDate = pc.PromoCode.BeginDate.ToString("yyyy-MM-dd"),
                        EndDate = pc.PromoCode.EndDate.ToString("yyyy-MM-dd"),
                        PartnerId = pc.PromoCode.PartnerId.ToString(),
                        ServiceInfo = pc.PromoCode.ServiceInfo
                    }));
                }

                return new GetCustomerResponse { Customer = grpcCustomer };
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении клиента {CustomerId} через gRPC", request.Id);
                throw new RpcException(new Status(StatusCode.Internal, "Внутренняя ошибка сервера"));
            }
        }

        public override async Task<CreateCustomerResponse> CreateCustomer(CreateCustomerRequest request, ServerCallContext context)
        {
            try
            {
                var preferenceIds = request.PreferenceIds
                    .Select(id => Guid.TryParse(id, out var guid) ? guid : Guid.Empty)
                    .Where(id => id != Guid.Empty)
                    .ToList();

                var preferences = await _preferenceRepository.GetRangeByIdsAsync(preferenceIds);

                var createRequest = new Models.CreateOrEditCustomerRequest
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    PreferenceIds = preferenceIds
                };

                var customer = CustomerMapper.MapFromModel(createRequest, preferences);
                await _customerRepository.AddAsync(customer);

                return new CreateCustomerResponse { Id = customer.Id.ToString() };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании клиента через gRPC");
                throw new RpcException(new Status(StatusCode.Internal, "Внутренняя ошибка сервера"));
            }
        }

        public override async Task<UpdateCustomerResponse> UpdateCustomer(UpdateCustomerRequest request, ServerCallContext context)
        {
            try
            {
                if (!Guid.TryParse(request.Id, out var customerId))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Некорректный ID клиента"));
                }

                var customer = await _customerRepository.GetByIdAsync(customerId);
                
                if (customer == null)
                {
                    throw new RpcException(new Status(StatusCode.NotFound, "Клиент не найден"));
                }

                var preferenceIds = request.PreferenceIds
                    .Select(id => Guid.TryParse(id, out var guid) ? guid : Guid.Empty)
                    .Where(id => id != Guid.Empty)
                    .ToList();

                var preferences = await _preferenceRepository.GetRangeByIdsAsync(preferenceIds);

                var updateRequest = new Models.CreateOrEditCustomerRequest
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    PreferenceIds = preferenceIds
                };

                CustomerMapper.MapFromModel(updateRequest, preferences, customer);
                await _customerRepository.UpdateAsync(customer);

                return new UpdateCustomerResponse { Success = true };
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении клиента {CustomerId} через gRPC", request.Id);
                throw new RpcException(new Status(StatusCode.Internal, "Внутренняя ошибка сервера"));
            }
        }

        public override async Task<DeleteCustomerResponse> DeleteCustomer(DeleteCustomerRequest request, ServerCallContext context)
        {
            try
            {
                if (!Guid.TryParse(request.Id, out var customerId))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Некорректный ID клиента"));
                }

                var customer = await _customerRepository.GetByIdAsync(customerId);
                
                if (customer == null)
                {
                    throw new RpcException(new Status(StatusCode.NotFound, "Клиент не найден"));
                }

                await _customerRepository.DeleteAsync(customer);

                return new DeleteCustomerResponse { Success = true };
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении клиента {CustomerId} через gRPC", request.Id);
                throw new RpcException(new Status(StatusCode.Internal, "Внутренняя ошибка сервера"));
            }
        }
    }
} 