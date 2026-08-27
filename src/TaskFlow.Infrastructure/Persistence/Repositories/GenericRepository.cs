using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.Interfaces;
using TaskFlow.Infrastructure.Persistence.Data;

namespace TaskFlow.Infrastructure.Persistence.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly AppDBContext _dbContext;
        private readonly DbSet<T> _dbSet;
        public GenericRepository(AppDBContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = _dbContext.Set<T>();
        }
        public async Task AddEntity(T entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity), "Entity can not be null");
            await _dbSet.AddAsync(entity);
            
            
        }

        public async Task DeleteEntity(string id)
        {            
            var entity = await _dbSet.FindAsync(id);
            if (entity == null) throw new ArgumentNullException(nameof(entity), "Entity can not be null");
            _dbSet.Remove(entity);
        }

        public async Task SaveChanges()
        {
           await _dbContext.SaveChangesAsync();
        }
    }
}
