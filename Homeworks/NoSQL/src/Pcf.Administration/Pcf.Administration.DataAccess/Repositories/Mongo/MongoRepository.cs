using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;
using Pcf.Administration.Core.Abstractions.Repositories;
using Pcf.Administration.Core.Domain;
using Pcf.Administration.DataAccess.MongoModels;

namespace Pcf.Administration.DataAccess.Repositories.Mongo
{
    public abstract class MongoRepository<TEntity, TMongoEntity> : IRepository<TEntity>
        where TEntity : BaseEntity
        where TMongoEntity : class
    {
        protected readonly IMongoCollection<TMongoEntity> Collection;
        
        protected MongoRepository(IMongoDatabaseSettings settings, string collectionName)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            Collection = database.GetCollection<TMongoEntity>(collectionName);
        }
        
        public abstract Task<IEnumerable<TEntity>> GetAllAsync();
        
        public abstract Task<TEntity> GetByIdAsync(Guid id);
        
        public abstract Task<IEnumerable<TEntity>> GetRangeByIdsAsync(List<Guid> ids);
        
        public abstract Task<TEntity> GetFirstWhere(Expression<Func<TEntity, bool>> predicate);
        
        public abstract Task<IEnumerable<TEntity>> GetWhere(Expression<Func<TEntity, bool>> predicate);

        public abstract Task AddAsync(TEntity entity);

        public abstract Task UpdateAsync(TEntity entity);

        public abstract Task DeleteAsync(TEntity entity);
    }
} 