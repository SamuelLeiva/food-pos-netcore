using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        protected readonly PosContext _context;

        public GenericRepository(PosContext context)
        {
            _context = context;
        }

        public virtual void Add(T entity)
        {
            _context.Set<T>().Add(entity);
        }

        public virtual void AddRange(IEnumerable<T> entities)
        {
            _context.Set<T>().AddRange(entities);
        }

        public virtual IEnumerable<T> Find(Expression<Func<T, bool>> expression, bool noTracking = true)
        {
            return noTracking ? _context.Set<T>().AsNoTracking().Where(expression)
                                : _context.Set<T>().Where(expression);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync(bool noTracking = true)
        {
            return noTracking ? await _context.Set<T>().AsNoTracking().ToListAsync()
                                : await _context.Set<T>().ToListAsync();
        }

        // se añade virtual´para colecciones sencillas sin entidades anidadas (en el caso de Product, sobrescribiremos)
        public virtual async Task<(int totalRegisters, IEnumerable<T> registers)> GetAllAsync(int pageIndex, int pageSize, string search, bool noTracking = true)
        {
            var query = noTracking ? _context.Set<T>().AsNoTracking().AsQueryable()
                                    : _context.Set<T>().AsQueryable();

            var totalRegisters = await query
                                .CountAsync();

            var registers = await query
                                    .Skip((pageIndex - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToListAsync();

            return (totalRegisters, registers);
        }


        public virtual async Task<T> GetByIdAsync(int id, bool noTracking = true)
        {
            var entity = await _context.Set<T>().FindAsync(id);

            if (noTracking)
            {
                _context.Entry(entity).State = EntityState.Detached;
            }
            return entity;
        }

        public virtual void Remove(T entity)
        {
            _context.Set<T>().Remove(entity);
        }

        public virtual void RemoveRange(IEnumerable<T> entities)
        {
            _context.Set<T>().RemoveRange(entities);
        }

        public virtual void Update(T entity)
        {
            _context.Set<T>()
                .Update(entity);
        }
    }

}
