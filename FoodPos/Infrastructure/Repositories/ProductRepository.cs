using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class ProductoRepository : GenericRepository<Product>, IProductRepository
    {
        public ProductoRepository(PosContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Product>> GetProductosMasCaros(int cantidad) =>
                        await _context.Products
                            .OrderByDescending(p => p.Price)
                            .Take(cantidad)
                            .ToListAsync();
    }

}
