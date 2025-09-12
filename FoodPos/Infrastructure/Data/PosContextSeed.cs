using Core.Entities;
using CsvHelper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data
{
    public class PosContextSeed
    {
        public static async Task SeedAsync(PosContext context, ILoggerFactory loggerFactory)
        {
            try
            {
                var route = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                if (!context.Categories.Any())
                {
                    using (var readerCategories = new StreamReader(route + @"/Data/Csvs/categories.csv"))
                    {
                        using (var csvCategories = new CsvReader(readerCategories, CultureInfo.InvariantCulture))
                        {
                            var categories = csvCategories.GetRecords<Category>();
                            context.Categories.AddRange(categories);
                            await context.SaveChangesAsync();
                        }
                    }
                }

                if (!context.Products.Any())
                {
                    using (var readerProducts = new StreamReader(route + @"/Data/Csvs/products.csv"))
                    {
                        using (var csvProducts = new CsvReader(readerProducts, CultureInfo.InvariantCulture))
                        {
                            var listProductsCsv = csvProducts.GetRecords<Product>();

                            List<Product> products = new List<Product>();
                            foreach (var item in listProductsCsv)
                            {
                                products.Add(new Product
                                {
                                    Id = item.Id,
                                    Name = item.Name,
                                    Description = item.Description,
                                    Price = item.Price,
                                    ImageUrl = item.ImageUrl,
                                    IsActive = item.IsActive,
                                    CategoryId = item.CategoryId,
                                    CreatedAt = item.CreatedAt,
                                    UpdatedAt = item.UpdatedAt
                                });
                            }

                            context.Products.AddRange(products);
                            await context.SaveChangesAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var logger = loggerFactory.CreateLogger<PosContextSeed>();
                logger.LogError(ex.Message);
            }
        }

        public static async Task SeedRolesAsync(PosContext context, ILoggerFactory loggerFactory)
        {
            try
            {
                if (!context.Roles.Any())
                {
                    var roles = new List<Role>()
                        {
                            new Role{Id=1, Name="Admin", CreatedAt=DateTime.UtcNow, UpdatedAt=DateTime.UtcNow },
                            new Role{Id=2, Name="Client", CreatedAt=DateTime.UtcNow, UpdatedAt=DateTime.UtcNow},
                        };
                    context.Roles.AddRange(roles);
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                var logger = loggerFactory.CreateLogger<PosContextSeed>();
                logger.LogError(ex.Message);
            }
        }

    }
}
