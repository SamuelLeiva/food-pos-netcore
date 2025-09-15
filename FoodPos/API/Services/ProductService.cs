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
    public async Task<Pager<ProductDto>> GetProductsAsync(Params productParams)
    {
        var result = await _unitOfWork.Products.GetAllAsync(productParams.PageIndex, productParams.PageSize, productParams.Search);
        var productsListDto = _mapper.Map<List<ProductDto>>(result.registers);
        return new Pager<ProductDto>(productsListDto, result.totalRegisters, productParams.PageIndex, productParams.PageSize, productParams.Search);
    }
    public async Task<ServiceResult<ProductDto>> GetProductByIdAsync(int id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null)
        {
            return ServiceResult<ProductDto>.Failure("The product requested does not exist.");
        }
        var productDto = _mapper.Map<ProductDto>(product);
        return ServiceResult<ProductDto>.Success(productDto);
    }
    public async Task<ServiceResult<ProductDto>> CreateProductAsync(ProductAddUpdateDto productDto)
    {
        var productExists = _unitOfWork.Products
            .Find(p => p.Name.ToLower() == productDto.Name.ToLower())
            .FirstOrDefault();

        if (productExists != null)
        {
            return ServiceResult<ProductDto>.Failure("A product with the same name already exists.");
        }

        var product = _mapper.Map<Product>(productDto);
        _unitOfWork.Products.Add(product);
        await _unitOfWork.SaveAsync();

        await _unitOfWork.Products.GetByIdAsync(product.Id);

        var createdProductDto = _mapper.Map<ProductDto>(product);

        return ServiceResult<ProductDto>.Success(createdProductDto);
    }
    public async Task<ServiceResult<ProductDto>> UpdateProductAsync(int id, ProductAddUpdateDto productDto)
    {
        var productDb = await _unitOfWork.Products.GetByIdAsync(id);
        if (productDb == null)
        {
            return ServiceResult<ProductDto>.Failure("The product requested does not exist.");
        }

        var productExists = _unitOfWork.Products
            .Find(p => p.Name.ToLower() == productDto.Name.ToLower() && p.Id != id)
            .FirstOrDefault();

        if (productExists != null)
        {
            return ServiceResult<ProductDto>.Failure("Another product with the same name already exists.");
        }

        // Mapea los valores del DTO de entrada a la entidad de la base de datos
        _mapper.Map(productDto, productDb);
        productDb.UpdatedAt = DateTime.Now;
        await _unitOfWork.SaveAsync();

        // Mapea la entidad actualizada a un DTO de respuesta antes de retornarlo
        var updatedProductDto = _mapper.Map<ProductDto>(productDb);

        return ServiceResult<ProductDto>.Success(updatedProductDto);
    }
    public async Task<ServiceResult> DeleteProductAsync(int id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null)
        {
            return ServiceResult.Failure("The product requested does not exist.");
        }

        _unitOfWork.Products.Remove(product);
        await _unitOfWork.SaveAsync();

        return ServiceResult.Success();
    }

    public async Task<Pager<ProductDto>> GetProductsByCategoryAsync(int categoryId, Params productParams)
    {
        var result = await _unitOfWork.Products.GetProductsByCategoryIdAsync(categoryId, productParams.PageIndex, productParams.PageSize, productParams.Search);
        var productsListDto = _mapper.Map<List<ProductDto>>(result.registers);
        return new Pager<ProductDto>(productsListDto, result.totalRegisters, productParams.PageIndex, productParams.PageSize, productParams.Search);
    }
}
