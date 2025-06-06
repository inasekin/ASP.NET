using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;
using Pcf.Administration.Core.Domain.Administration;
using Pcf.Administration.DataAccess.MongoModels;

namespace Pcf.Administration.DataAccess.Repositories.Mongo
{
    public class MongoEmployeeRepository : MongoRepository<Employee, MongoEmployee>
    {
        private readonly IMongoCollection<MongoRole> _rolesCollection;
        
        public MongoEmployeeRepository(IMongoDatabaseSettings settings) 
            : base(settings, settings.EmployeesCollectionName)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _rolesCollection = database.GetCollection<MongoRole>(settings.RolesCollectionName);
        }

        public override async Task<IEnumerable<Employee>> GetAllAsync()
        {
            var employees = await Collection.Find(_ => true).ToListAsync();
            return employees.Select(MapToEntity);
        }

        public override async Task<Employee> GetByIdAsync(Guid id)
        {
            var employee = await Collection.Find(e => e.Id == id).FirstOrDefaultAsync();
            return employee == null ? null : MapToEntity(employee);
        }

        public override async Task<IEnumerable<Employee>> GetRangeByIdsAsync(List<Guid> ids)
        {
            var employees = await Collection.Find(e => ids.Contains(e.Id)).ToListAsync();
            return employees.Select(MapToEntity);
        }

        public override async Task<Employee> GetFirstWhere(Expression<Func<Employee, bool>> predicate)
        {
            // Этот метод сложнее реализовать напрямую, так как предикат для Entity, а не для MongoEntity
            // Для простоты получаем все сущности и фильтруем их в памяти
            var employees = await GetAllAsync();
            return employees.FirstOrDefault(predicate.Compile());
        }

        public override async Task<IEnumerable<Employee>> GetWhere(Expression<Func<Employee, bool>> predicate)
        {
            // Тот же подход, что и выше
            var employees = await GetAllAsync();
            return employees.Where(predicate.Compile());
        }

        public override async Task AddAsync(Employee entity)
        {
            var mongoEmployee = MapToMongoEntity(entity);
            await Collection.InsertOneAsync(mongoEmployee);
        }

        public override async Task UpdateAsync(Employee entity)
        {
            var mongoEmployee = MapToMongoEntity(entity);
            await Collection.ReplaceOneAsync(e => e.Id == entity.Id, mongoEmployee);
        }

        public override async Task DeleteAsync(Employee entity)
        {
            await Collection.DeleteOneAsync(e => e.Id == entity.Id);
        }

        private Employee MapToEntity(MongoEmployee mongoEmployee)
        {
            var employee = new Employee
            {
                Id = mongoEmployee.Id,
                FirstName = mongoEmployee.FirstName,
                LastName = mongoEmployee.LastName,
                Email = mongoEmployee.Email,
                RoleId = mongoEmployee.RoleId,
                AppliedPromocodesCount = mongoEmployee.AppliedPromocodesCount
            };

            var role = _rolesCollection.Find(r => r.Id == mongoEmployee.RoleId).FirstOrDefault();
            if (role != null)
            {
                employee.Role = new Role
                {
                    Id = role.Id,
                    Name = role.Name,
                    Description = role.Description
                };
            }

            return employee;
        }

        private MongoEmployee MapToMongoEntity(Employee employee)
        {
            return new MongoEmployee
            {
                Id = employee.Id,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Email = employee.Email,
                RoleId = employee.RoleId,
                AppliedPromocodesCount = employee.AppliedPromocodesCount
            };
        }
    }
} 