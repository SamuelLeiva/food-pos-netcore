using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories;

public class OrderRepository : GenericRepository<Order>, IOrderRepository
{
    public OrderRepository(PosContext context) : base(context)
    {
    }

    //sobrescribimos los gets para que traigan los valores deseados y no null
    public override async Task<Order> GetByIdAsync(int id)
    {
        return await _context.Orders
                        .Include(o => o.OrderItems)
                        .FirstOrDefaultAsync(o => o.Id == id);
    }

    public override async Task<(int totalRegisters, IEnumerable<Order> registers)> GetAllAsync(int pageIndex, int pageSize, string search)
    {
        var query = _context.Orders as IQueryable<Order>;

        if (!String.IsNullOrEmpty(search))
        {
            query = query.Where(o => o.UserId.ToString().Contains(search));
        }

        var totalRegisters = await query
                                    .CountAsync();

        var registers = await query
                                .Include(o => o.OrderItems)
                                .Skip((pageIndex - 1) * pageSize)
                                .Take(pageSize)
                                .ToListAsync();

        return (totalRegisters, registers);
    }


    public async Task<(int totalRegisters, IEnumerable<Order> registers)> GetOrdersByUserIdAsync(int userId, int pageIndex, int pageSize, string search)
    {
        var query = _context.Orders as IQueryable<Order>;

        // buscamos por id de user
        if (!String.IsNullOrEmpty(search))
        {
            query = query.Where(o => o.UserId.ToString().Contains(search));
        }

        query = query.Where(o => o.UserId == userId);

        var totalRegisters = await query
                                    .CountAsync();

        var registers = await query
                                .Include(o => o.OrderItems)
                                .Skip((pageIndex - 1) * pageSize)
                                .Take(pageSize)
                                .ToListAsync();

        return (totalRegisters, registers);
    }
}
