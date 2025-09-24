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
    public async Task<ActionResult<List<CategoryDto>>> Get()
    {
        var result = await _categoryService.GetCategoriesAsync();
        if (result.IsSuccess)
            return Ok(result.Data);

        return BadRequest(new ApiResponse(400, result.ErrorMessage));
    }

    [HttpGet("paginated")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Pager<CategoryDto>>> Get([FromQuery] Params categoryParams)
    {
        var result = await _categoryService.GetCategoriesPaginatedAsync(categoryParams);
        if (result.IsSuccess)
            return Ok(result.Data);

        return BadRequest(new ApiResponse(400, result.ErrorMessage));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryDto>> Get(int id)
    {
        var result = await _categoryService.GetCategoryByIdAsync(id);

        if (result.IsSuccess)
            return Ok(result.Data);

        return NotFound(new ApiResponse(404, result.ErrorMessage));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Category>> Post(CategoryAddUpdateDto categoryDto)
    {
        var result = await _categoryService.CreateCategoryAsync(categoryDto);

        if (result.IsSuccess)
            return CreatedAtAction(nameof(Get), new { id = result.Data.Id }, result.Data);

        return Conflict(new ApiResponse(409, result.ErrorMessage));
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

        if (result.IsSuccess)
            return Ok(result.Data);

        // Dependiendo del error, retorna un 404 o un 409
        if (result.ErrorMessage.Contains("does not exist"))
        {
            return NotFound(new ApiResponse(404, result.ErrorMessage));
        }
        return Conflict(new ApiResponse(409, result.ErrorMessage));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _categoryService.DeleteCategoryAsync(id);
        if (result.IsSuccess)
            return NoContent(); // Retorna 204 si la operación fue exitosa

        return NotFound(new ApiResponse(404, result.ErrorMessage));
    }


}
