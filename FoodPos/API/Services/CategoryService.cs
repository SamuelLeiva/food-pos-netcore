using API.Dtos.Categories;
using API.Helpers;
using API.Services.Interfaces;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Core.Services;

namespace API.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CategoryService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }
    public async Task<ServiceResult<CategoryDto>> CreateCategoryAsync(CategoryAddUpdateDto categoryDto)
    {
        var categoryExists = _unitOfWork.Categories
            .Find(c => c.Name.ToLower() == categoryDto.Name.ToLower())
            .FirstOrDefault();

        if (categoryExists != null)
            return ServiceResult<CategoryDto>.Failure("A category with the same name already exists.");

        var category = _mapper.Map<Category>(categoryDto);
        _unitOfWork.Categories.Add(category);
        await _unitOfWork.SaveAsync();

        await _unitOfWork.Categories.GetByIdAsync(category.Id);
        var createdCategoryDto = _mapper.Map<CategoryDto>(category);
        return ServiceResult<CategoryDto>.Success(createdCategoryDto);
    }

    public async Task<ServiceResult> DeleteCategoryAsync(int id)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        if (category == null)
            return ServiceResult.Failure("The category to delete does not exist.");

        _unitOfWork.Categories.Remove(category);
        await _unitOfWork.SaveAsync();

        return ServiceResult.Success();
    }

    public async Task<Pager<CategoryDto>> GetCategoriesAsync(Params categoryParams)
    {
        var result = await _unitOfWork.Categories.GetAllAsync(categoryParams.PageIndex, categoryParams.PageSize, categoryParams.Search);
        var categoriesListDto = _mapper.Map<List<CategoryDto>>(result.registers);
        return new Pager<CategoryDto>(categoriesListDto, result.totalRegisters, categoryParams.PageIndex, categoryParams.PageSize, categoryParams.Search);
    }

    public async Task<ServiceResult<CategoryDto>> GetCategoryByIdAsync(int id)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        if (category == null)
            return ServiceResult<CategoryDto>.Failure("The category requested does not exist.");
        var categoryDto = _mapper.Map<CategoryDto>(category);
        return ServiceResult<CategoryDto>.Success(categoryDto);
    }

    public async Task<ServiceResult<CategoryDto>> UpdateCategoryAsync(int id, CategoryAddUpdateDto categoryDto)
    {
        var categoryDb = await _unitOfWork.Categories.GetByIdAsync(id);
        if (categoryDb == null)
            return ServiceResult<CategoryDto>.Failure("The category requested does not exist.");

        var categoryExists = _unitOfWork.Categories
            .Find(c => c.Name.ToLower() == categoryDto.Name.ToLower() && c.Id != id)
            .FirstOrDefault();

        if (categoryExists != null)
            return ServiceResult<CategoryDto>.Failure("A category with the same name already exists.");

        _mapper.Map(categoryDto, categoryDb);
        categoryDb.UpdatedAt = DateTime.Now;
        await _unitOfWork.SaveAsync();

        var updatedCategoryDto = _mapper.Map<CategoryDto>(categoryDb);
        return ServiceResult<CategoryDto>.Success(updatedCategoryDto);
    }
}
