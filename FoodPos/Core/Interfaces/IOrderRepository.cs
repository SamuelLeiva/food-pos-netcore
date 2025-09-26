using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces;

public interface IOrderRepository : IGenericRepository<Order>
{
    Task<(int totalRegisters, IEnumerable<Order> registers)> GetOrdersByUserIdAsync(int userId, int pageIndex, int pageSize, string search);
}
