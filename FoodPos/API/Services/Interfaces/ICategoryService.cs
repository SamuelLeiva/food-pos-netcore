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
    Task<Pager<CategoryDto>> GetCategoriesAsync(Params categoryParams);
    Task<ServiceResult<CategoryDto>> GetCategoryByIdAsync(int id);
}
