using Data.Context;
using DataAccess.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository.Implementation
{
   public class Repository<T> : IRepository<T> where T:class
   {
         readonly EnforcerContext enforcerContext;
         DbSet<T> table;

        public Repository(EnforcerContext enforcerContext)
        {
            this.enforcerContext = enforcerContext;
            table = enforcerContext.Set<T>();
        }

        public async Task Create(T entity)
        {
            await table.AddAsync(entity);
        }

        public async Task CreateMany(List<T> entity)
        {
            await table.AddRangeAsync(entity);
        }

        public async Task Delete(Guid id)
        {
            var entity = await GetByID(id);
            table.Remove(entity);
        }

        public int DeleteObject(T entity)
        {
            table.Remove(entity);
            return 1;
        }

        public async Task<IEnumerable<T>> Find(Expression<Func<T, bool>> predicate)
        {
            return await table.Where(predicate).ToListAsync();
        }

        public async Task<IEnumerable<T>> GetAll()
        {
            return await table.AsNoTracking().ToListAsync();
        }

        public IQueryable<T> GetAllQuery()
        {
            return table.AsQueryable();
        }

        public async Task<T> GetByID(Guid id)
        {
            return await table.FindAsync(id);
        }

        public void Update(T entity)
        {
            table.Attach(entity);
            enforcerContext.Entry(entity).State = EntityState.Modified;
        }

        public void UpdateEntityRange(List<T> entities)
        {
            table.UpdateRange(entities);
        }
    }
}
