using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using Npgsql;
using Pcf.Administration.Core.Abstractions.Repositories;
using Pcf.Administration.Core.Domain.Administration;
using Pcf.Administration.DataAccess;
using Pcf.Administration.DataAccess.MongoModels;
using Pcf.Administration.DataAccess.Services;

namespace Pcf.Administration.WebHost.Controllers
{
    [ApiController]
    [Route("api/v1/migration")]
    public class MigrationController : ControllerBase
    {
        private readonly DataContext _dataContext;
        private readonly IMongoDatabaseSettings _mongoSettings;
        private readonly ILogger<MigrationController> _logger;

        public MigrationController(
            DataContext dataContext,
            IMongoDatabaseSettings mongoSettings,
            ILogger<MigrationController> logger)
        {
            _dataContext = dataContext;
            _mongoSettings = mongoSettings;
            _logger = logger;
        }

        /// <summary>
        /// Создает структуру таблиц в PostgreSQL
        /// </summary>
        [HttpPost("create-schema")]
        public async Task<IActionResult> CreateSchema()
        {
            try
            {
                _logger.LogInformation("Создание схемы базы данных PostgreSQL...");
                
                // Получаем строку подключения из контекста
                var connectionString = _dataContext.Database.GetConnectionString();
                _logger.LogInformation($"Строка подключения: {connectionString}");
                
                // Проверяем, существует ли база данных
                using var connection = new NpgsqlConnection(connectionString);
                await connection.OpenAsync();
                
                // Выводим информацию о подключении
                _logger.LogInformation($"Подключено к базе данных: {connection.Database} на сервере {connection.Host}:{connection.Port}");
                
                // Пробуем создать схему если ее нет
                using (var createSchemaCommand = new NpgsqlCommand("CREATE SCHEMA IF NOT EXISTS public;", connection))
                {
                    await createSchemaCommand.ExecuteNonQueryAsync();
                }
                
                // Создаем таблицы с явным указанием схемы
                using (var createTablesCommand = new NpgsqlCommand(@"
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
                ", connection))
                {
                    await createTablesCommand.ExecuteNonQueryAsync();
                }
                
                // Проверяем, созданы ли таблицы
                using (var checkTablesCommand = new NpgsqlCommand(@"
                    SELECT EXISTS (
                        SELECT FROM information_schema.tables 
                        WHERE table_schema = 'public' 
                        AND table_name = 'Roles'
                    ) as roles_exists;
                ", connection))
                {
                    var rolesExist = (bool)await checkTablesCommand.ExecuteScalarAsync();
                    _logger.LogInformation($"Таблица 'Roles' существует: {rolesExist}");
                }
                
                _logger.LogInformation("Схема базы данных PostgreSQL успешно создана");
                return Ok("Схема базы данных PostgreSQL успешно создана");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании схемы базы данных: " + ex.Message);
                return StatusCode(500, "Ошибка при создании схемы: " + ex.Message);
            }
        }
        
        /// <summary>
        /// Добавляет тестовые данные в PostgreSQL
        /// </summary>
        [HttpPost("seed-data")]
        public async Task<IActionResult> SeedData()
        {
            try
            {
                _logger.LogInformation("Добавление тестовых данных в PostgreSQL...");
                
                // Получаем строку подключения из контекста
                var connectionString = _dataContext.Database.GetConnectionString();
                
                using var connection = new NpgsqlConnection(connectionString);
                await connection.OpenAsync();
                
                // Проверяем, есть ли данные в таблице ролей
                try
                {
                    using (var checkCommand = new NpgsqlCommand("SELECT COUNT(*) FROM public.\"Roles\"", connection))
                    {
                        var count = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());
                        
                        if (count > 0)
                        {
                            _logger.LogInformation("В базе уже есть данные. Пропускаем добавление тестовых данных.");
                            return Ok("В базе уже есть данные. Пропускаем добавление тестовых данных.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Ошибка при проверке данных: {ex.Message}. Продолжаем добавление тестовых данных.");
                }
                
                // Генерируем идентификаторы для ролей
                var adminRoleId = Guid.NewGuid();
                var managerRoleId = Guid.NewGuid();
                
                // Добавляем роли
                using (var rolesCommand = new NpgsqlCommand(@"
                    INSERT INTO public.""Roles"" (""Id"", ""Name"", ""Description"") VALUES 
                    (@adminRoleId, 'Administrator', 'System administrator'),
                    (@managerRoleId, 'Manager', 'Company manager');
                ", connection))
                {
                    rolesCommand.Parameters.AddWithValue("adminRoleId", adminRoleId);
                    rolesCommand.Parameters.AddWithValue("managerRoleId", managerRoleId);
                    await rolesCommand.ExecuteNonQueryAsync();
                }
                
                // Добавляем сотрудников
                using (var employeesCommand = new NpgsqlCommand(@"
                    INSERT INTO public.""Employees"" (""Id"", ""FirstName"", ""LastName"", ""Email"", ""RoleId"", ""AppliedPromocodesCount"") VALUES 
                    (@adminId, 'Admin', 'User', 'admin@example.com', @adminRoleId, 0),
                    (@managerId, 'Manager', 'User', 'manager@example.com', @managerRoleId, 0);
                ", connection))
                {
                    employeesCommand.Parameters.AddWithValue("adminId", Guid.NewGuid());
                    employeesCommand.Parameters.AddWithValue("managerId", Guid.NewGuid());
                    employeesCommand.Parameters.AddWithValue("adminRoleId", adminRoleId);
                    employeesCommand.Parameters.AddWithValue("managerRoleId", managerRoleId);
                    await employeesCommand.ExecuteNonQueryAsync();
                }
                
                _logger.LogInformation("Тестовые данные успешно добавлены в PostgreSQL");
                return Ok("Тестовые данные успешно добавлены в PostgreSQL");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении тестовых данных: " + ex.Message);
                return StatusCode(500, "Ошибка при добавлении тестовых данных: " + ex.Message);
            }
        }
        
        /// <summary>
        /// Мигрирует данные из PostgreSQL в MongoDB
        /// </summary>
        [HttpPost("migrate-to-mongo")]
        public async Task<IActionResult> MigrateToMongo()
        {
            try
            {
                _logger.LogInformation("Миграция данных из PostgreSQL в MongoDB...");
                
                // Получаем строку подключения из контекста
                var connectionString = _dataContext.Database.GetConnectionString();
                
                using var connection = new NpgsqlConnection(connectionString);
                await connection.OpenAsync();
                
                // Получаем роли из PostgreSQL
                var roles = new List<Role>();
                using (var rolesCommand = new NpgsqlCommand("SELECT \"Id\", \"Name\", \"Description\" FROM public.\"Roles\"", connection))
                {
                    using var reader = await rolesCommand.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        roles.Add(new Role
                        {
                            Id = reader.GetGuid(0),
                            Name = reader.GetString(1),
                            Description = !reader.IsDBNull(2) ? reader.GetString(2) : null
                        });
                    }
                }
                
                _logger.LogInformation($"Получено {roles.Count} ролей из PostgreSQL");
                
                // Получаем сотрудников из PostgreSQL
                var employees = new List<Employee>();
                using (var employeesCommand = new NpgsqlCommand(@"
                    SELECT e.""Id"", e.""FirstName"", e.""LastName"", e.""Email"", e.""RoleId"", e.""AppliedPromocodesCount"" 
                    FROM public.""Employees"" e
                ", connection))
                {
                    using var reader = await employeesCommand.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        employees.Add(new Employee
                        {
                            Id = reader.GetGuid(0),
                            FirstName = reader.GetString(1),
                            LastName = reader.GetString(2),
                            Email = reader.GetString(3),
                            RoleId = reader.GetGuid(4),
                            AppliedPromocodesCount = reader.GetInt32(5)
                        });
                    }
                }
                
                _logger.LogInformation($"Получено {employees.Count} сотрудников из PostgreSQL");
                
                // Настраиваем MongoDB
                _logger.LogInformation($"MongoDB ConnectionString: {_mongoSettings.ConnectionString}");
                _logger.LogInformation($"MongoDB DatabaseName: {_mongoSettings.DatabaseName}");
                
                var client = new MongoClient(_mongoSettings.ConnectionString);
                var database = client.GetDatabase(_mongoSettings.DatabaseName);
                
                // Создаем коллекции в MongoDB (или очищаем существующие)
                var rolesCollection = database.GetCollection<MongoRole>(_mongoSettings.RolesCollectionName);
                var employeesCollection = database.GetCollection<MongoEmployee>(_mongoSettings.EmployeesCollectionName);
                
                await rolesCollection.DeleteManyAsync(Builders<MongoRole>.Filter.Empty);
                await employeesCollection.DeleteManyAsync(Builders<MongoEmployee>.Filter.Empty);
                
                // Мигрируем роли в MongoDB
                if (roles.Count > 0)
                {
                    var mongoRoles = new List<MongoRole>();
                    foreach (var role in roles)
                    {
                        mongoRoles.Add(new MongoRole
                        {
                            Id = role.Id,
                            Name = role.Name,
                            Description = role.Description
                        });
                    }
                    
                    await rolesCollection.InsertManyAsync(mongoRoles);
                    _logger.LogInformation($"Мигрировано {mongoRoles.Count} ролей в MongoDB");
                }
                
                // Мигрируем сотрудников в MongoDB
                if (employees.Count > 0)
                {
                    var mongoEmployees = new List<MongoEmployee>();
                    foreach (var employee in employees)
                    {
                        mongoEmployees.Add(new MongoEmployee
                        {
                            Id = employee.Id,
                            FirstName = employee.FirstName,
                            LastName = employee.LastName,
                            Email = employee.Email,
                            RoleId = employee.RoleId,
                            AppliedPromocodesCount = employee.AppliedPromocodesCount
                        });
                    }
                    
                    await employeesCollection.InsertManyAsync(mongoEmployees);
                    _logger.LogInformation($"Мигрировано {mongoEmployees.Count} сотрудников в MongoDB");
                }
                
                _logger.LogInformation("Миграция данных из PostgreSQL в MongoDB успешно завершена");
                return Ok("Миграция данных из PostgreSQL в MongoDB успешно завершена");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при миграции данных в MongoDB: " + ex.Message);
                return StatusCode(500, "Ошибка при миграции данных в MongoDB: " + ex.Message);
            }
        }
        
        /// <summary>
        /// Выполняет все шаги миграции последовательно
        /// </summary>
        [HttpPost("start")]
        public async Task<IActionResult> StartMigration()
        {
            try
            {
                // Шаг 1: Создание схемы
                var schemaResult = await CreateSchema();
                if (schemaResult is ObjectResult { StatusCode: 500 })
                {
                    return schemaResult;
                }
                
                // Шаг 2: Добавление тестовых данных
                var seedResult = await SeedData();
                if (seedResult is ObjectResult { StatusCode: 500 })
                {
                    return seedResult;
                }
                
                // Шаг 3: Миграция данных в MongoDB
                var migrateResult = await MigrateToMongo();
                if (migrateResult is ObjectResult { StatusCode: 500 })
                {
                    return migrateResult;
                }
                
                return Ok("Миграция успешно выполнена");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при выполнении миграции: " + ex.Message);
                return StatusCode(500, "Ошибка при выполнении миграции: " + ex.Message);
            }
        }
        
        /// <summary>
        /// Проверка подключения к MongoDB и создание тестовых данных
        /// </summary>
        [HttpPost("test-mongo")]
        public async Task<IActionResult> TestMongo()
        {
            try
            {
                _logger.LogInformation("Проверка подключения к MongoDB...");
                _logger.LogInformation($"MongoDB ConnectionString: {_mongoSettings.ConnectionString}");
                _logger.LogInformation($"MongoDB DatabaseName: {_mongoSettings.DatabaseName}");
                _logger.LogInformation($"MongoDB Collections: {_mongoSettings.RolesCollectionName}, {_mongoSettings.EmployeesCollectionName}");
                
                var client = new MongoClient(_mongoSettings.ConnectionString);
                var database = client.GetDatabase(_mongoSettings.DatabaseName);
                
                // Проверяем доступные базы данных
                var dbNames = new List<string>();
                using (var cursor = await client.ListDatabaseNamesAsync())
                {
                    while (await cursor.MoveNextAsync())
                    {
                        dbNames.AddRange(cursor.Current);
                    }
                }
                
                _logger.LogInformation($"Доступные базы данных MongoDB: {string.Join(", ", dbNames)}");
                
                // Создаем коллекции
                if (!await CollectionExistsAsync(database, _mongoSettings.RolesCollectionName))
                {
                    await database.CreateCollectionAsync(_mongoSettings.RolesCollectionName);
                    _logger.LogInformation($"Коллекция {_mongoSettings.RolesCollectionName} создана");
                }
                else
                {
                    _logger.LogInformation($"Коллекция {_mongoSettings.RolesCollectionName} уже существует");
                }
                
                if (!await CollectionExistsAsync(database, _mongoSettings.EmployeesCollectionName))
                {
                    await database.CreateCollectionAsync(_mongoSettings.EmployeesCollectionName);
                    _logger.LogInformation($"Коллекция {_mongoSettings.EmployeesCollectionName} создана");
                }
                else
                {
                    _logger.LogInformation($"Коллекция {_mongoSettings.EmployeesCollectionName} уже существует");
                }
                
                // Добавляем тестовые данные в MongoDB
                var rolesCollection = database.GetCollection<MongoRole>(_mongoSettings.RolesCollectionName);
                var employeesCollection = database.GetCollection<MongoEmployee>(_mongoSettings.EmployeesCollectionName);
                
                // Очищаем коллекции
                await rolesCollection.DeleteManyAsync(Builders<MongoRole>.Filter.Empty);
                await employeesCollection.DeleteManyAsync(Builders<MongoEmployee>.Filter.Empty);
                
                // Создаем тестовые роли
                var adminRole = new MongoRole
                {
                    Id = Guid.NewGuid(),
                    Name = "Admin",
                    Description = "Administrator role"
                };
                
                var userRole = new MongoRole
                {
                    Id = Guid.NewGuid(),
                    Name = "User",
                    Description = "Regular user role"
                };
                
                await rolesCollection.InsertManyAsync(new[] { adminRole, userRole });
                _logger.LogInformation("Тестовые роли добавлены в MongoDB");
                
                // Создаем тестовых сотрудников
                var employee1 = new MongoEmployee
                {
                    Id = Guid.NewGuid(),
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe@example.com",
                    RoleId = adminRole.Id,
                    AppliedPromocodesCount = 5
                };
                
                var employee2 = new MongoEmployee
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Jane",
                    LastName = "Smith",
                    Email = "jane.smith@example.com",
                    RoleId = userRole.Id,
                    AppliedPromocodesCount = 2
                };
                
                await employeesCollection.InsertManyAsync(new[] { employee1, employee2 });
                _logger.LogInformation("Тестовые сотрудники добавлены в MongoDB");
                
                // Проверяем, что данные действительно добавились
                var rolesCount = await rolesCollection.CountDocumentsAsync(Builders<MongoRole>.Filter.Empty);
                var employeesCount = await employeesCollection.CountDocumentsAsync(Builders<MongoEmployee>.Filter.Empty);
                
                _logger.LogInformation($"В MongoDB {rolesCount} ролей и {employeesCount} сотрудников");
                
                return Ok(new
                {
                    Message = "Тестовые данные успешно добавлены в MongoDB",
                    Databases = dbNames,
                    RolesCount = rolesCount,
                    EmployeesCount = employeesCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при тестировании MongoDB: " + ex.Message);
                return StatusCode(500, "Ошибка при тестировании MongoDB: " + ex.Message);
            }
        }
        
        /// <summary>
        /// Проверка подключения к PostgreSQL
        /// </summary>
        [HttpPost("test-postgres")]
        public async Task<IActionResult> TestPostgres()
        {
            try
            {
                _logger.LogInformation("Проверка подключения к PostgreSQL...");
                
                // Получаем строку подключения из контекста
                var connectionString = _dataContext.Database.GetConnectionString();
                _logger.LogInformation($"EntityFramework ConnectionString: {connectionString}");
                
                // Используем явную строку подключения из конфигурации
                var explicitConnectionString = "Host=promocode-factory-administration-db;Port=5432;Database=promocode_factory_administration_db;Username=postgres;Password=docker";
                _logger.LogInformation($"Explicit ConnectionString: {explicitConnectionString}");
                
                // Проверяем подключение с явной строкой подключения
                using var connection = new NpgsqlConnection(explicitConnectionString);
                await connection.OpenAsync();
                _logger.LogInformation($"Подключено к базе данных: {connection.Database} на сервере {connection.Host}:{connection.Port}");
                
                // Проверяем список схем
                var schemas = new List<string>();
                using (var schemaCommand = new NpgsqlCommand("SELECT schema_name FROM information_schema.schemata", connection))
                {
                    using var reader = await schemaCommand.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        schemas.Add(reader.GetString(0));
                    }
                }
                
                _logger.LogInformation($"Доступные схемы: {string.Join(", ", schemas)}");
                
                // Проверяем список таблиц
                var tables = new List<string>();
                using (var tableCommand = new NpgsqlCommand(@"
                    SELECT table_schema, table_name 
                    FROM information_schema.tables 
                    WHERE table_schema NOT IN ('pg_catalog', 'information_schema')
                ", connection))
                {
                    using var reader = await tableCommand.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        tables.Add($"{reader.GetString(0)}.{reader.GetString(1)}");
                    }
                }
                
                _logger.LogInformation($"Доступные таблицы: {string.Join(", ", tables)}");
                
                // Создаем схему public, если ее нет
                using (var createSchemaCommand = new NpgsqlCommand("CREATE SCHEMA IF NOT EXISTS public", connection))
                {
                    await createSchemaCommand.ExecuteNonQueryAsync();
                    _logger.LogInformation("Схема public создана или уже существует");
                }
                
                // Создаем таблицы roles и employees
                using (var createRolesTableCommand = new NpgsqlCommand(@"
                    CREATE TABLE IF NOT EXISTS roles (
                        id UUID PRIMARY KEY,
                        name VARCHAR(100) NOT NULL,
                        description VARCHAR(255) NULL
                    )
                ", connection))
                {
                    await createRolesTableCommand.ExecuteNonQueryAsync();
                    _logger.LogInformation("Таблица roles создана или уже существует");
                }
                
                using (var createEmployeesTableCommand = new NpgsqlCommand(@"
                    CREATE TABLE IF NOT EXISTS employees (
                        id UUID PRIMARY KEY,
                        first_name VARCHAR(100) NOT NULL,
                        last_name VARCHAR(100) NOT NULL,
                        email VARCHAR(255) NOT NULL,
                        role_id UUID NOT NULL,
                        applied_promocodes_count INT NOT NULL DEFAULT 0,
                        CONSTRAINT fk_role FOREIGN KEY(role_id) REFERENCES roles(id) ON DELETE CASCADE
                    )
                ", connection))
                {
                    await createEmployeesTableCommand.ExecuteNonQueryAsync();
                    _logger.LogInformation("Таблица employees создана или уже существует");
                }
                
                // Добавляем тестовые данные
                var adminRoleId = Guid.NewGuid();
                var userRoleId = Guid.NewGuid();
                
                using (var insertRolesCommand = new NpgsqlCommand(@"
                    INSERT INTO roles (id, name, description) VALUES 
                    (@adminRoleId, 'Admin', 'Administrator role'),
                    (@userRoleId, 'User', 'Regular user role')
                    ON CONFLICT (id) DO NOTHING
                ", connection))
                {
                    insertRolesCommand.Parameters.AddWithValue("adminRoleId", adminRoleId);
                    insertRolesCommand.Parameters.AddWithValue("userRoleId", userRoleId);
                    await insertRolesCommand.ExecuteNonQueryAsync();
                    _logger.LogInformation("Роли добавлены");
                }
                
                using (var insertEmployeesCommand = new NpgsqlCommand(@"
                    INSERT INTO employees (id, first_name, last_name, email, role_id, applied_promocodes_count) VALUES 
                    (@employee1Id, 'John', 'Doe', 'john.doe@example.com', @adminRoleId, 5),
                    (@employee2Id, 'Jane', 'Smith', 'jane.smith@example.com', @userRoleId, 2)
                    ON CONFLICT (id) DO NOTHING
                ", connection))
                {
                    insertEmployeesCommand.Parameters.AddWithValue("employee1Id", Guid.NewGuid());
                    insertEmployeesCommand.Parameters.AddWithValue("employee2Id", Guid.NewGuid());
                    insertEmployeesCommand.Parameters.AddWithValue("adminRoleId", adminRoleId);
                    insertEmployeesCommand.Parameters.AddWithValue("userRoleId", userRoleId);
                    await insertEmployeesCommand.ExecuteNonQueryAsync();
                    _logger.LogInformation("Сотрудники добавлены");
                }
                
                // Проверяем данные
                var rolesCount = 0;
                using (var countRolesCommand = new NpgsqlCommand("SELECT COUNT(*) FROM roles", connection))
                {
                    rolesCount = Convert.ToInt32(await countRolesCommand.ExecuteScalarAsync());
                }
                
                var employeesCount = 0;
                using (var countEmployeesCommand = new NpgsqlCommand("SELECT COUNT(*) FROM employees", connection))
                {
                    employeesCount = Convert.ToInt32(await countEmployeesCommand.ExecuteScalarAsync());
                }
                
                _logger.LogInformation($"В PostgreSQL {rolesCount} ролей и {employeesCount} сотрудников");
                
                return Ok(new
                {
                    Message = "Подключение к PostgreSQL успешно",
                    ConnectionString = connectionString,
                    ExplicitConnectionString = explicitConnectionString,
                    DatabaseName = connection.Database,
                    Host = connection.Host,
                    Port = connection.Port,
                    Schemas = schemas,
                    Tables = tables,
                    RolesCount = rolesCount,
                    EmployeesCount = employeesCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при тестировании PostgreSQL: " + ex.Message);
                return StatusCode(500, "Ошибка при тестировании PostgreSQL: " + ex.Message);
            }
        }
        
        /// <summary>
        /// Создает тестовые данные напрямую в MongoDB (без миграции из PostgreSQL)
        /// </summary>
        [HttpPost("create-demo-data")]
        public async Task<IActionResult> CreateDemoData()
        {
            try
            {
                _logger.LogInformation("Создание тестовых данных напрямую в MongoDB...");
                
                // Проверка настроек подключения к MongoDB
                _logger.LogInformation($"MongoDB ConnectionString: {_mongoSettings.ConnectionString}");
                _logger.LogInformation($"MongoDB DatabaseName: {_mongoSettings.DatabaseName}");
                _logger.LogInformation($"MongoDB Collections: {_mongoSettings.RolesCollectionName}, {_mongoSettings.EmployeesCollectionName}");
                
                // Подключаемся к MongoDB
                var client = new MongoClient(_mongoSettings.ConnectionString);
                var database = client.GetDatabase(_mongoSettings.DatabaseName);
                
                // Создаем коллекции (если их еще нет)
                if (!await CollectionExistsAsync(database, _mongoSettings.RolesCollectionName))
                {
                    await database.CreateCollectionAsync(_mongoSettings.RolesCollectionName);
                    _logger.LogInformation($"Коллекция {_mongoSettings.RolesCollectionName} создана");
                }
                
                if (!await CollectionExistsAsync(database, _mongoSettings.EmployeesCollectionName))
                {
                    await database.CreateCollectionAsync(_mongoSettings.EmployeesCollectionName);
                    _logger.LogInformation($"Коллекция {_mongoSettings.EmployeesCollectionName} создана");
                }
                
                // Получаем коллекции
                var rolesCollection = database.GetCollection<MongoRole>(_mongoSettings.RolesCollectionName);
                var employeesCollection = database.GetCollection<MongoEmployee>(_mongoSettings.EmployeesCollectionName);
                
                // Очищаем существующие данные
                await rolesCollection.DeleteManyAsync(Builders<MongoRole>.Filter.Empty);
                await employeesCollection.DeleteManyAsync(Builders<MongoEmployee>.Filter.Empty);
                
                // Создаем роли
                var roles = new List<MongoRole>
                {
                    new MongoRole
                    {
                        Id = Guid.NewGuid(),
                        Name = "Administrator",
                        Description = "Администратор системы"
                    },
                    new MongoRole
                    {
                        Id = Guid.NewGuid(),
                        Name = "Manager",
                        Description = "Менеджер"
                    },
                    new MongoRole
                    {
                        Id = Guid.NewGuid(),
                        Name = "Employee",
                        Description = "Сотрудник"
                    }
                };
                
                // Добавляем роли в базу
                await rolesCollection.InsertManyAsync(roles);
                _logger.LogInformation($"Создано {roles.Count} ролей в MongoDB");
                
                // Создаем сотрудников
                var employees = new List<MongoEmployee>
                {
                    new MongoEmployee
                    {
                        Id = Guid.NewGuid(),
                        FirstName = "Иван",
                        LastName = "Иванов",
                        Email = "ivan@example.com",
                        RoleId = roles[0].Id, // Administrator
                        AppliedPromocodesCount = 10
                    },
                    new MongoEmployee
                    {
                        Id = Guid.NewGuid(),
                        FirstName = "Петр",
                        LastName = "Петров",
                        Email = "petr@example.com",
                        RoleId = roles[1].Id, // Manager
                        AppliedPromocodesCount = 5
                    },
                    new MongoEmployee
                    {
                        Id = Guid.NewGuid(),
                        FirstName = "Сергей",
                        LastName = "Сергеев",
                        Email = "sergey@example.com",
                        RoleId = roles[2].Id, // Employee
                        AppliedPromocodesCount = 2
                    }
                };
                
                // Добавляем сотрудников в базу
                await employeesCollection.InsertManyAsync(employees);
                _logger.LogInformation($"Создано {employees.Count} сотрудников в MongoDB");
                
                return Ok(new
                {
                    Message = "Тестовые данные успешно созданы в MongoDB",
                    Roles = roles.Select(r => new { r.Id, r.Name }),
                    Employees = employees.Select(e => new { e.Id, e.FirstName, e.LastName, e.Email, RoleName = roles.First(r => r.Id == e.RoleId).Name })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании тестовых данных в MongoDB: " + ex.Message);
                return StatusCode(500, "Ошибка при создании тестовых данных в MongoDB: " + ex.Message);
            }
        }
        
        private async Task<bool> CollectionExistsAsync(IMongoDatabase database, string collectionName)
        {
            var filter = new BsonDocument("name", collectionName);
            var collections = await database.ListCollectionsAsync(new ListCollectionsOptions { Filter = filter });
            return await collections.AnyAsync();
        }
    }
} 