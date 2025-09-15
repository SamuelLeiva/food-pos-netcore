using API.Dtos.Categories;
using API.Helpers;
using API.Helpers.Errors;
using API.Services.Interfaces;
using Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class CategoriesController : BaseApiController
{
    private readonly ICategoryService _categoryService;
    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Pager<CategoryDto>>> Get([FromQuery] Params categoryParams)
    {
        var result = await _categoryService.GetCategoriesAsync(categoryParams);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryDto>> Get(int id)
    {
        var result = await _categoryService.GetCategoryByIdAsync(id);
        if (!result.IsSuccess)
        {
            return NotFound(new ApiResponse(404, result.ErrorMessage));
        }

        return Ok(result.Data);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Category>> Post(CategoryAddUpdateDto categoryDto)
    {
        var result = await _categoryService.CreateCategoryAsync(categoryDto);
        if (!result.IsSuccess)
        {
            return Conflict(new ApiResponse(409, result.ErrorMessage));
        }

        return CreatedAtAction(nameof(Post), result.Data);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CategoryDto>> Put(int id, [FromBody] CategoryAddUpdateDto categoryDto)
    {
        var result = await _categoryService.UpdateCategoryAsync(id, categoryDto);
        if (!result.IsSuccess)
        {
            return NotFound(new ApiResponse(404, result.ErrorMessage));
        }

        return Ok(result.Data);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _categoryService.DeleteCategoryAsync(id);
        if (!result.IsSuccess)
        {
            return NotFound(new ApiResponse(404, result.ErrorMessage));
        }

        return NoContent();
    }


}
