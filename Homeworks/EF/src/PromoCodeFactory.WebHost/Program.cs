using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PromoCodeFactory.DataAccess.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;

namespace PromoCodeFactory.WebHost
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<DataContext>();

                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                AddRoles(db);
                AddPreferences(db);
                AddEmployees(db);
                AddCustomers(db);
                AddPromoCodes(db);

                db.SaveChanges();
            }

            host.Run();
        }

        private static void AddRoles(DataContext db)
        {
            foreach (var role in FakeDataFactory.Roles)
            {
                if (!db.Roles.Any(r => r.Id == role.Id))
                {
                    db.Roles.Add(role);
                }
            }
        }

        private static void AddPreferences(DataContext db)
        {
            foreach (var preference in FakeDataFactory.Preferences)
            {
                if (!db.Preferences.Any(p => p.Id == preference.Id))
                {
                    db.Preferences.Add(preference);
                }
            }

            db.SaveChanges();
        }

        private static void AddEmployees(DataContext db)
        {
            foreach (var employee in FakeDataFactory.Employees)
            {
                if (!db.Employees.Any(e => e.Id == employee.Id))
                {
                    var role = db.Roles.FirstOrDefault(r => r.Id == employee.Role.Id);
                    if (role == null)
                    {
                        throw new InvalidOperationException($"Роль с Id {employee.Role.Id} не найдена.");
                    }

                    employee.Role = role;
                    db.Employees.Add(employee);
                }
            }

            db.SaveChanges();
        }

        private static void AddCustomers(DataContext db)
        {
            var customers = FakeDataFactory.Customers.ToList();
            var preferences = db.Preferences.AsNoTracking().ToList();

            if (!preferences.Any())
            {
                throw new InvalidOperationException("Таблица предпочтений пуста. Невозможно назначить предпочтения клиентам.");
            }

            foreach (var customer in customers)
            {
                if (!db.Customers.Any(c => c.Id == customer.Id))
                {
                    var firstPreference = preferences.FirstOrDefault();
                    if (firstPreference != null)
                    {
                        customer.CustomerPreferences = new List<CustomerPreference>
                        {
                            new CustomerPreference
                            {
                                CustomerId = customer.Id,
                                PreferenceId = firstPreference.Id
                            }
                        };
                    }

                    db.Customers.Add(customer);
                }
            }
        }
        
        private static void AddPromoCodes(DataContext db)
        {
            var preferences = FakeDataFactory.Preferences.ToList();
            var customers = FakeDataFactory.Customers.ToList();

            if (!preferences.Any())
            {
                throw new InvalidOperationException("Таблица предпочтений пуста. Невозможно назначить предпочтения промокодам.");
            }

            foreach (var customer in customers)
            {
                if (!db.PromoCodes.Any(pc => pc.CustomerId == customer.Id))
                {
                    var preference = preferences.FirstOrDefault();

                    if (preference != null && customer != null)
                    {
                        var promoCode = new PromoCode
                        {
                            Id = Guid.NewGuid(),
                            Code = $"PROMO-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
                            ServiceInfo = "Тестовая услуга",
                            BeginDate = DateTime.Now,
                            EndDate = DateTime.Now.AddMonths(1),
                            PreferenceId = preference.Id,
                            CustomerId = customer.Id
                        };

                        db.PromoCodes.Add(promoCode);
                    }
                }
            }

            db.SaveChanges();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}
