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
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        public ProductRepository(PosContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Product>> GetMostExpensiveProducts(int quantity) =>
                        await _context.Products
                            .OrderByDescending(p => p.Price)
                            .Take(quantity)
                            .ToListAsync();

        // Sobrescribimos el metodo para que incluya a category y no aparezca null en la respuesta
        public override async Task<Product> GetByIdAsync(int id, bool noTracking = true)
        {
            var queryProduct = noTracking ? _context.Products.AsNoTracking()
                                        : _context.Products;

            return await queryProduct
                            .Include(p => p.Category)
                            .FirstOrDefaultAsync(p => p.Id == id);
        }

        public override async Task<IEnumerable<Product>> GetAllAsync(bool noTracking = true)
        {
            var queryProduct = noTracking ? _context.Products.AsNoTracking()
                                        : _context.Products;

            return await queryProduct
                .Include(p => p.Category)
                .ToListAsync();
        }

        public override async Task<(int totalRegisters, IEnumerable<Product> registers)> GetAllAsync(int pageIndex, int pageSize, string search, bool noTracking = true)
        {
            var queryProduct = noTracking ? _context.Products.AsNoTracking()
                                        : _context.Products;

            if (!String.IsNullOrEmpty(search))
            {
                queryProduct = queryProduct.Where(p => p.Name.ToLower().Contains(search));
            }

            var totalRegisters = await queryProduct
                                        .CountAsync();

            var registers = await queryProduct
                                    .Include(p => p.Category)
                                    .Skip((pageIndex - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToListAsync();

            return (totalRegisters, registers);
        }

    }

}
