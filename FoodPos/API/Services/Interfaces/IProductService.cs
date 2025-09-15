using API.Dtos.Products;
using API.Helpers;
using Core.Services;

namespace API.Services.Interfaces;

public interface IProductService
{
    Task<ServiceResult<ProductDto>> CreateProductAsync(ProductAddUpdateDto productDto);
    Task<ServiceResult<ProductDto>> UpdateProductAsync(int id, ProductAddUpdateDto productDto);
    Task<ServiceResult> DeleteProductAsync(int id);
    Task<Pager<ProductDto>> GetProductsAsync(Params productParams);
    Task<ServiceResult<ProductDto>> GetProductByIdAsync(int id);
    Task<Pager<ProductDto>> GetProductsByCategoryAsync(int categoryId ,Params productParams);
}
