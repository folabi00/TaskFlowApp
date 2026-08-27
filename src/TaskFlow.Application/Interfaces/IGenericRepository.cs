using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskFlow.Application.Interfaces
{
    public interface IGenericRepository <T> where T : class
    {
        Task AddEntity(T entity);
        Task DeleteEntity(string entity);
        Task SaveChanges();
    }
}
