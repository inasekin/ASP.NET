using System;
using System.Collections.Generic;
using System.Linq;
using PromoCodeFactory.Core.Domain.Administration;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;

namespace PromoCodeFactory.DataAccess.Data
{
    public static class FakeDataFactory
    {
        private static readonly Lazy<List<Role>> _roles = new(() => new List<Role>
        {
            new Role
            {
                Id = Guid.Parse("53729686-a368-4eeb-8bfa-cc69b6050d02"),
                Name = "Admin",
                Description = "Администратор",
            },
            new Role
            {
                Id = Guid.Parse("b0ae7aac-5493-45cd-ad16-87426a5e7665"),
                Name = "PartnerManager",
                Description = "Партнёрский менеджер"
            }
        });

        private static readonly Lazy<List<Employee>> _employees = new(() => new List<Employee>
        {
            new Employee
            {
                Id = Guid.Parse("451533d5-d8d5-4a11-9c7b-eb9f14e1a32f"),
                Email = "owner@somemail.ru",
                FirstName = "Иван",
                LastName = "Сергеев",
                Role = Roles.First(x => x.Name == "Admin"),
                AppliedPromocodesCount = 5
            },
            new Employee
            {
                Id = Guid.Parse("f766e2bf-340a-46ea-bff3-f1700b435895"),
                Email = "andreev@somemail.ru",
                FirstName = "Петр",
                LastName = "Андреев",
                Role = Roles.First(x => x.Name == "PartnerManager"),
                AppliedPromocodesCount = 10
            },
        });

        private static readonly Lazy<List<Preference>> _preferences = new(() => new List<Preference>
        {
            new Preference
            {
                Id = Guid.Parse("ef7f299f-92d7-459f-896e-078ed53ef99c"),
                Name = "Театр",
            },
            new Preference
            {
                Id = Guid.Parse("c4bda62e-fc74-4256-a956-4760b3858cbd"),
                Name = "Семья",
            },
            new Preference
            {
                Id = Guid.Parse("76324c47-68d2-472d-abb8-33cfa8cc0c84"),
                Name = "Дети",
            }
        });

        private static readonly Lazy<List<Customer>> _customers = new(() =>
        {
            var customerId = Guid.Parse("a6c8c6b1-4349-45b0-ab31-244740aaf0f0");
            return new List<Customer>
            {
                new Customer
                {
                    Id = customerId,
                    Email = "ivan_sergeev@mail.ru",
                    FirstName = "Иван",
                    LastName = "Петров",
                    //TODO: Добавить предзаполненный список предпочтений
                }
            };
        });
        
        private static readonly Lazy<List<PromoCode>> _promoCodes = new(() => new List<PromoCode>
        {
            new PromoCode
            {
                Id = Guid.NewGuid(),
                Code = "TEST2024",
                ServiceInfo = "Тестовая услуга",
                BeginDate = DateTime.Now,
                EndDate = DateTime.Now.AddMonths(1),
                PreferenceId = Preferences.First().Id,
            }
        });

        public static IEnumerable<PromoCode> PromoCodes => _promoCodes.Value;
        public static IEnumerable<Role> Roles => _roles.Value;
        public static IEnumerable<Employee> Employees => _employees.Value;
        public static IEnumerable<Preference> Preferences => _preferences.Value;
        public static IEnumerable<Customer> Customers => _customers.Value;
    }
}
