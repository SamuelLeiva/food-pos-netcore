using API.Dtos.Products;
using API.Helpers;
using API.Services.Interfaces;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Core.Services;

namespace API.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ProductService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ServiceResult<Pager<ProductDto>>> GetProductsAsync(Params productParams)
    {
        try
        {
            var result = await _unitOfWork.Products.GetAllAsync(productParams.PageIndex, productParams.PageSize, productParams.Search);
            var productsListDto = _mapper.Map<List<ProductDto>>(result.registers);
            var pager = new Pager<ProductDto>(productsListDto, result.totalRegisters, productParams.PageIndex, productParams.PageSize, productParams.Search);
            return ServiceResult<Pager<ProductDto>>.Success(pager);
        }
        catch (Exception ex)
        {
            // 500 Internal Server Error: Para errores inesperados de la base de datos o lógica.
            return ServiceResult<Pager<ProductDto>>.Failure($"An unexpected error occurred while retrieving products: {ex.Message}", 500);
        }
    }

    public async Task<ServiceResult<ProductDto>> GetProductByIdAsync(int id)
    {
        try
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
            {
                // 404 Not Found: El recurso solicitado no existe.
                return ServiceResult<ProductDto>.Failure("The product requested does not exist.", 404);
            }
            var productDto = _mapper.Map<ProductDto>(product);
            return ServiceResult<ProductDto>.Success(productDto);
        }
        catch (Exception ex)
        {
            // 500 Internal Server Error
            return ServiceResult<ProductDto>.Failure($"An unexpected error occurred while retrieving the product: {ex.Message}", 500);
        }
    }

    public async Task<ServiceResult<ProductDto>> CreateProductAsync(ProductAddUpdateDto productDto)
    {
        try
        {
            var productExists = _unitOfWork.Products
                .Find(p => p.Name.ToLower() == productDto.Name.ToLower())
                .FirstOrDefault();

            if (productExists != null)
            {
                // 409 Conflict: El recurso que intentas crear ya existe.
                return ServiceResult<ProductDto>.Failure("A product with the same name already exists.", 409);
            }

            var product = _mapper.Map<Product>(productDto);
            _unitOfWork.Products.Add(product);
            await _unitOfWork.SaveAsync();

            await _unitOfWork.Products.GetByIdAsync(product.Id);

            var createdProductDto = _mapper.Map<ProductDto>(product);
            return ServiceResult<ProductDto>.Success(createdProductDto);
        }
        catch (Exception ex)
        {
            // 500 Internal Server Error
            return ServiceResult<ProductDto>.Failure($"An unexpected error occurred while creating the product: {ex.Message}", 500);
        }
    }

    public async Task<ServiceResult<ProductDto>> UpdateProductAsync(int id, ProductAddUpdateDto productDto)
    {
        try
        {
            var productDb = await _unitOfWork.Products.GetByIdAsync(id);
            if (productDb == null)
            {
                // 404 Not Found: El recurso a actualizar no existe.
                return ServiceResult<ProductDto>.Failure("The product requested does not exist.", 404);
            }

            var productExists = _unitOfWork.Products
                .Find(p => p.Name.ToLower() == productDto.Name.ToLower() && p.Id != id)
                .FirstOrDefault();

            if (productExists != null)
            {
                // 409 Conflict: Intentaste renombrar el producto a un nombre que ya está en uso.
                return ServiceResult<ProductDto>.Failure("Another product with the same name already exists.", 409);
            }

            _mapper.Map(productDto, productDb);
            productDb.UpdatedAt = DateTime.Now;
            await _unitOfWork.SaveAsync();

            var updatedProductDto = _mapper.Map<ProductDto>(productDb);
            return ServiceResult<ProductDto>.Success(updatedProductDto);
        }
        catch (Exception ex)
        {
            // 500 Internal Server Error
            return ServiceResult<ProductDto>.Failure($"An unexpected error occurred while updating the product: {ex.Message}", 500);
        }
    }

    public async Task<ServiceResult> DeleteProductAsync(int id)
    {
        try
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
            {
                // 404 Not Found: El recurso a eliminar no existe.
                return ServiceResult.Failure("The product requested does not exist.", 404);
            }

            _unitOfWork.Products.Remove(product);
            await _unitOfWork.SaveAsync();
            return ServiceResult.Success();
        }
        catch (Exception ex)
        {
            // 500 Internal Server Error
            return ServiceResult.Failure($"An unexpected error occurred while deleting the product: {ex.Message}", 500);
        }
    }

    public async Task<ServiceResult<Pager<ProductDto>>> GetProductsByCategoryAsync(int categoryId, Params productParams)
    {
        try
        {
            var result = await _unitOfWork.Products.GetProductsByCategoryIdAsync(categoryId, productParams.PageIndex, productParams.PageSize, productParams.Search);
            var productsListDto = _mapper.Map<List<ProductDto>>(result.registers);
            var pager = new Pager<ProductDto>(productsListDto, result.totalRegisters, productParams.PageIndex, productParams.PageSize, productParams.Search);
            return ServiceResult<Pager<ProductDto>>.Success(pager);
        }
        catch (Exception ex)
        {
            // 500 Internal Server Error
            return ServiceResult<Pager<ProductDto>>.Failure($"An unexpected error occurred while retrieving the products: {ex.Message}", 500);
        }
    }
}