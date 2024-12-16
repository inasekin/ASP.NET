using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain;

namespace PromoCodeFactory.DataAccess.Repositories
{
    public class InMemoryRepository<T>(IEnumerable<T> data) : IRepository<T>
        where T : BaseEntity
    {
        private List<T> Data { get; set; } = data.ToList();

        public Task<IEnumerable<T>> GetAllAsync()
        {
            return Task.FromResult(Data.AsEnumerable());
        }

        public Task<T> GetByIdAsync(Guid id)
        {
            return Task.FromResult(Data.FirstOrDefault(x => x.Id == id));
        }

        public Task<T> AddAsync(T entity)
        {
            entity.Id = Guid.NewGuid();
            Data.Add(entity);
            return Task.FromResult(entity);
        }

        public Task<T> UpdateAsync(T entity)
        {
            var existing = Data.FirstOrDefault(x => x.Id == entity.Id);
            if (existing != null)
            {
                Data.Remove(existing);
                Data.Add(entity);
            }
            return Task.FromResult(entity);
        }

        public Task DeleteAsync(T entity)
        {
            Data.Remove(entity);
            return Task.CompletedTask;
        }
    }
}