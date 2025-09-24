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
        try
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
        catch (Exception ex)
        {
            return ServiceResult<CategoryDto>.Failure($"An error occurred while creating the category: {ex.Message}");
        }
        
    }

    public async Task<ServiceResult> DeleteCategoryAsync(int id)
    {
        try
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            if (category == null)
                return ServiceResult.Failure("The category to delete does not exist.");

            _unitOfWork.Categories.Remove(category);
            await _unitOfWork.SaveAsync();

            return ServiceResult.Success();
        }
        catch (Exception ex)
        {
            return ServiceResult.Failure($"An error occurred while deleting the category: {ex.Message}");
        }
        
    }

    public async Task<ServiceResult<Pager<CategoryDto>>> GetCategoriesPaginatedAsync(Params categoryParams)
    {
        try
        {
            var result = await _unitOfWork.Categories.GetAllAsync(categoryParams.PageIndex, categoryParams.PageSize, categoryParams.Search);
            var categoriesListDto = _mapper.Map<List<CategoryDto>>(result.registers);
            var pager = new Pager<CategoryDto>(categoriesListDto, result.totalRegisters, categoryParams.PageIndex, categoryParams.PageSize, categoryParams.Search);
            return ServiceResult<Pager<CategoryDto>>.Success(pager);
        }
        catch (Exception ex)
        {
            return ServiceResult<Pager<CategoryDto>>.Failure($"An error occurred while retrieving paginated categories: {ex.Message}");
        }
    }

    public async Task<ServiceResult<List<CategoryDto>>> GetCategoriesAsync()
    {
        try
        {
            var result = await _unitOfWork.Categories.GetAllAsync();
            var categoriesListDto = _mapper.Map<List<CategoryDto>>(result);
            return ServiceResult<List<CategoryDto>>.Success(categoriesListDto);
        }
        catch (Exception ex)
        {
            return ServiceResult<List<CategoryDto>>.Failure($"An error occurred while retrieving all categories: {ex.Message}");
        }
    }

    public async Task<ServiceResult<CategoryDto>> GetCategoryByIdAsync(int id)
    {
        try
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            if (category == null)
                return ServiceResult<CategoryDto>.Failure("The category requested does not exist.");

            var categoryDto = _mapper.Map<CategoryDto>(category);
            return ServiceResult<CategoryDto>.Success(categoryDto);
        }
        catch (Exception ex)
        {
            return ServiceResult<CategoryDto>.Failure($"An error occurred while retrieving the category: {ex.Message}");
        }
    }

    public async Task<ServiceResult<CategoryDto>> UpdateCategoryAsync(int id, CategoryAddUpdateDto categoryDto)
    {
        try
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
        catch (Exception ex)
        {
            return ServiceResult<CategoryDto>.Failure($"An error occurred while updating the category: {ex.Message}");
        }
    }
}
