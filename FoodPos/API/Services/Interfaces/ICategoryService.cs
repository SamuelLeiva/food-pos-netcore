using API.Dtos.Categories;
using API.Dtos.Products;
using API.Helpers;
using Core.Services;

namespace API.Services.Interfaces;

public interface ICategoryService
{
    Task<ServiceResult<CategoryDto>> CreateCategoryAsync(CategoryAddUpdateDto categoryDto);
    Task<ServiceResult<CategoryDto>> UpdateCategoryAsync(int id, CategoryAddUpdateDto categoryDto);
    Task<ServiceResult> DeleteCategoryAsync(int id);
    Task<ServiceResult<List<CategoryDto>>> GetCategoriesAsync();
    Task<ServiceResult<Pager<CategoryDto>>> GetCategoriesPaginatedAsync(Params categoryParams);
    Task<ServiceResult<CategoryDto>> GetCategoryByIdAsync(int id);
}
