using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IGenericRepository<T> where T : BaseEntity
    {
        Task<T> GetByIdAsync(int id, bool noTracking = true);
        Task<IEnumerable<T>> GetAllAsync(bool noTracking = true);
        IEnumerable<T> Find(Expression<Func<T, bool>> expression, bool noTracking = true);
        Task<(int totalRegisters, IEnumerable<T> registers)> GetAllAsync(int pageIndex, int pageSize, string search, bool noTracking = true);
        void Add(T entity);
        void AddRange(IEnumerable<T> entities);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);
        void Update(T entity);
    }

}
