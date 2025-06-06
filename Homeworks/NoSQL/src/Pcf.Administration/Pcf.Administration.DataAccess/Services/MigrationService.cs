using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pcf.Administration.Core.Abstractions.Repositories;
using Pcf.Administration.Core.Domain.Administration;
using Pcf.Administration.DataAccess.Repositories.Mongo;

namespace Pcf.Administration.DataAccess.Services
{
    public class MigrationService : IMigrationService
    {
        private readonly DataContext _dataContext;
        private readonly IRepository<Employee> _employeeRepository;
        private readonly IRepository<Role> _roleRepository;
        private readonly ILogger<MigrationService> _logger;

        public MigrationService(
            DataContext dataContext,
            IRepository<Employee> employeeRepository,
            IRepository<Role> roleRepository,
            ILogger<MigrationService> logger)
        {
            _dataContext = dataContext;
            _employeeRepository = employeeRepository;
            _roleRepository = roleRepository;
            _logger = logger;
        }

        public async Task MigrateDataAsync()
        {
            _logger.LogInformation("Начинаем миграцию данных из PostgreSQL в MongoDB...");

            // Проверяем, что база данных создана и доступна
            await EnsureDatabaseCreatedWithRetryAsync();
            
            // Создаем схему, если её нет
            await CreateSchemaIfNotExistAsync();
            
            // Создаем таблицы, если их нет
            await CreateTablesIfNotExistAsync();
            
            // Добавляем тестовые данные, если нужно
            await AddTestDataIfNeededAsync();
            
            // Мигрируем данные из PostgreSQL в MongoDB
            await MigratePostgresToMongoAsync();

            _logger.LogInformation("Миграция данных завершена успешно");
        }
        
        private async Task EnsureDatabaseCreatedWithRetryAsync(int maxRetryCount = 5, int retryIntervalMs = 2000)
        {
            int retryCount = 0;
            while (true)
            {
                try
                {
                    _logger.LogInformation("Попытка подключения к PostgreSQL...");
                    await _dataContext.Database.OpenConnectionAsync();
                    await _dataContext.Database.CloseConnectionAsync();
                    _logger.LogInformation("Подключение к PostgreSQL успешно установлено");
                    
                    // Если нет исключений, значит подключение успешно
                    return;
                }
                catch (Exception ex)
                {
                    retryCount++;
                    if (retryCount >= maxRetryCount)
                    {
                        _logger.LogError(ex, $"Не удалось подключиться к PostgreSQL после {maxRetryCount} попыток. Последняя ошибка: {ex.Message}");
                        throw;
                    }
                    
                    _logger.LogWarning($"Попытка {retryCount} не удалась. Ошибка: {ex.Message}. Повторная попытка через {retryIntervalMs}мс...");
                    await Task.Delay(retryIntervalMs);
                }
            }
        }
        
        private async Task CreateSchemaIfNotExistAsync()
        {
            try
            {
                _logger.LogInformation("Создание схемы public если не существует...");
                
                await _dataContext.Database.ExecuteSqlRawAsync(@"
                    CREATE SCHEMA IF NOT EXISTS public;
                ");
                
                _logger.LogInformation("Схема public создана или уже существует");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании схемы: " + ex.Message);
                throw;
            }
        }
        
        private async Task CreateTablesIfNotExistAsync()
        {
            try
            {
                _logger.LogInformation("Проверка и создание таблиц...");
                
                await _dataContext.Database.ExecuteSqlRawAsync(@"
                    CREATE TABLE IF NOT EXISTS public.""Roles"" (
                        ""Id"" UUID PRIMARY KEY,
                        ""Name"" VARCHAR(100) NOT NULL,
                        ""Description"" VARCHAR(255) NULL
                    );
                    
                    CREATE TABLE IF NOT EXISTS public.""Employees"" (
                        ""Id"" UUID PRIMARY KEY,
                        ""FirstName"" VARCHAR(100) NOT NULL,
                        ""LastName"" VARCHAR(100) NOT NULL,
                        ""Email"" VARCHAR(255) NOT NULL,
                        ""RoleId"" UUID NOT NULL,
                        ""AppliedPromocodesCount"" INT NOT NULL DEFAULT 0,
                        CONSTRAINT fk_role FOREIGN KEY(""RoleId"") REFERENCES public.""Roles""(""Id"") ON DELETE CASCADE
                    );
                ");
                
                _logger.LogInformation("Таблицы успешно созданы или уже существуют");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании таблиц: " + ex.Message);
                throw;
            }
        }
        
        private async Task AddTestDataIfNeededAsync()
        {
            try
            {
                // Проверяем, есть ли данные в таблице ролей
                int rolesCount = 0;
                try
                {
                    // Пытаемся выполнить запрос через прямой SQL с явным указанием схемы
                    var rawCountSql = "SELECT COUNT(*) FROM public.\"Roles\"";
                    using var command = _dataContext.Database.GetDbConnection().CreateCommand();
                    command.CommandText = rawCountSql;
                    
                    // Открываем подключение если оно закрыто
                    if (command.Connection.State != System.Data.ConnectionState.Open)
                    {
                        await command.Connection.OpenAsync();
                    }
                    
                    // Выполняем запрос
                    var result = await command.ExecuteScalarAsync();
                    if (result != null && result != DBNull.Value)
                    {
                        rolesCount = Convert.ToInt32(result);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Ошибка при проверке данных: {ex.Message}");
                    rolesCount = 0;
                }
                
                _logger.LogInformation($"Найдено {rolesCount} существующих ролей");
                
                // Если данных нет, добавляем тестовые данные
                if (rolesCount == 0)
                {
                    _logger.LogInformation("Добавление тестовых данных...");
                    
                    // Генерируем ID для ролей
                    var adminRoleId = Guid.NewGuid();
                    var managerRoleId = Guid.NewGuid();
                    
                    // Добавляем роли
                    await _dataContext.Database.ExecuteSqlRawAsync($@"
                        INSERT INTO public.""Roles"" (""Id"", ""Name"", ""Description"") VALUES 
                        ('{adminRoleId}', 'Administrator', 'System administrator'),
                        ('{managerRoleId}', 'Manager', 'Company manager');
                    ");
                    
                    // Добавляем сотрудников
                    await _dataContext.Database.ExecuteSqlRawAsync($@"
                        INSERT INTO public.""Employees"" (""Id"", ""FirstName"", ""LastName"", ""Email"", ""RoleId"", ""AppliedPromocodesCount"") VALUES 
                        ('{Guid.NewGuid()}', 'Admin', 'User', 'admin@example.com', '{adminRoleId}', 0),
                        ('{Guid.NewGuid()}', 'Manager', 'User', 'manager@example.com', '{managerRoleId}', 0);
                    ");
                    
                    _logger.LogInformation("Тестовые данные успешно добавлены");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении тестовых данных: " + ex.Message);
                // Не выбрасываем исключение, продолжаем выполнение
            }
        }
        
        private async Task MigratePostgresToMongoAsync()
        {
            try
            {
                // Миграция ролей
                _logger.LogInformation("Миграция ролей из PostgreSQL в MongoDB...");
                
                var roles = new List<Role>();
                using (var command = _dataContext.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = "SELECT \"Id\", \"Name\", \"Description\" FROM public.\"Roles\"";
                    
                    if (command.Connection.State != System.Data.ConnectionState.Open)
                    {
                        await command.Connection.OpenAsync();
                    }
                    
                    using var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        var role = new Role
                        {
                            Id = reader.GetGuid(0),
                            Name = reader.GetString(1),
                            Description = !reader.IsDBNull(2) ? reader.GetString(2) : null
                        };
                        roles.Add(role);
                    }
                }
                
                _logger.LogInformation($"Найдено {roles.Count} ролей для миграции");
                
                foreach (var role in roles)
                {
                    await _roleRepository.AddAsync(role);
                    _logger.LogInformation($"Роль '{role.Name}' успешно мигрирована");
                }
                
                // Миграция сотрудников
                _logger.LogInformation("Миграция сотрудников из PostgreSQL в MongoDB...");
                
                var employees = new List<Employee>();
                using (var command = _dataContext.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = @"
                        SELECT e.""Id"", e.""FirstName"", e.""LastName"", e.""Email"", e.""RoleId"", e.""AppliedPromocodesCount"",
                               r.""Name"" as ""RoleName"", r.""Description"" as ""RoleDescription""
                        FROM public.""Employees"" e
                        JOIN public.""Roles"" r ON e.""RoleId"" = r.""Id""
                    ";
                    
                    if (command.Connection.State != System.Data.ConnectionState.Open)
                    {
                        await command.Connection.OpenAsync();
                    }
                    
                    using var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        var employee = new Employee
                        {
                            Id = reader.GetGuid(0),
                            FirstName = reader.GetString(1),
                            LastName = reader.GetString(2),
                            Email = reader.GetString(3),
                            RoleId = reader.GetGuid(4),
                            AppliedPromocodesCount = reader.GetInt32(5),
                            Role = new Role
                            {
                                Id = reader.GetGuid(4),
                                Name = reader.GetString(6),
                                Description = !reader.IsDBNull(7) ? reader.GetString(7) : null
                            }
                        };
                        employees.Add(employee);
                    }
                }
                
                _logger.LogInformation($"Найдено {employees.Count} сотрудников для миграции");
                
                foreach (var employee in employees)
                {
                    await _employeeRepository.AddAsync(employee);
                    _logger.LogInformation($"Сотрудник '{employee.FullName}' успешно мигрирован");
                }
                
                _logger.LogInformation("Миграция из PostgreSQL в MongoDB успешно завершена");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при миграции данных: " + ex.Message);
                throw;
            }
        }
    }

    public interface IMigrationService
    {
        Task MigrateDataAsync();
    }
} 