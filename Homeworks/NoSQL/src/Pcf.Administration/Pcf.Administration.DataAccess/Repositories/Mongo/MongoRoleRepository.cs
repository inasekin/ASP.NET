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
    public class MongoRoleRepository : MongoRepository<Role, MongoRole>
    {
        public MongoRoleRepository(IMongoDatabaseSettings settings) 
            : base(settings, settings.RolesCollectionName)
        {
        }

        public override async Task<IEnumerable<Role>> GetAllAsync()
        {
            var roles = await Collection.Find(_ => true).ToListAsync();
            return roles.Select(MapToEntity);
        }

        public override async Task<Role> GetByIdAsync(Guid id)
        {
            var role = await Collection.Find(r => r.Id == id).FirstOrDefaultAsync();
            return role == null ? null : MapToEntity(role);
        }

        public override async Task<IEnumerable<Role>> GetRangeByIdsAsync(List<Guid> ids)
        {
            var roles = await Collection.Find(r => ids.Contains(r.Id)).ToListAsync();
            return roles.Select(MapToEntity);
        }

        public override async Task<Role> GetFirstWhere(Expression<Func<Role, bool>> predicate)
        {
            // Для простоты получаем все роли и фильтруем их в памяти
            var roles = await GetAllAsync();
            return roles.FirstOrDefault(predicate.Compile());
        }

        public override async Task<IEnumerable<Role>> GetWhere(Expression<Func<Role, bool>> predicate)
        {
            var roles = await GetAllAsync();
            return roles.Where(predicate.Compile());
        }

        public override async Task AddAsync(Role entity)
        {
            var mongoRole = MapToMongoEntity(entity);
            await Collection.InsertOneAsync(mongoRole);
        }

        public override async Task UpdateAsync(Role entity)
        {
            var mongoRole = MapToMongoEntity(entity);
            await Collection.ReplaceOneAsync(r => r.Id == entity.Id, mongoRole);
        }

        public override async Task DeleteAsync(Role entity)
        {
            await Collection.DeleteOneAsync(r => r.Id == entity.Id);
        }

        private Role MapToEntity(MongoRole mongoRole)
        {
            return new Role
            {
                Id = mongoRole.Id,
                Name = mongoRole.Name,
                Description = mongoRole.Description
            };
        }

        private MongoRole MapToMongoEntity(Role role)
        {
            return new MongoRole
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description
            };
        }
    }
} 