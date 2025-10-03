using API.Dtos.Categories;
using API.Extensions;
using API.Helpers;
using API.Helpers.Errors;
using API.Helpers.Response;
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

    // El método ToActionResult maneja el 200 (Success) y el 500 (Catch/Error)
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)] // Usamos 500 para errores de servicio
    public async Task<ActionResult<List<CategoryDto>>> Get()
    {
        var result = await _categoryService.GetCategoriesAsync();
        // Control total: retorna 200 OK o 500 ObjectResult con el cuerpo ApiResponse
        return result.ToActionResult();
    }

    [HttpGet("paginated")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Pager<CategoryDto>>> Get([FromQuery] Params categoryParams)
    {
        var result = await _categoryService.GetCategoriesPaginatedAsync(categoryParams);
        // Control total
        return result.ToActionResult();
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)] // Error de Service (Category not found)
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CategoryDto>> Get(int id)
    {
        var result = await _categoryService.GetCategoryByIdAsync(id);
        // Control total: retorna 200 OK, 404 Not Found o 500 Internal Server Error
        return result.ToActionResult();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)] // Error de Service (Duplicate name)
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CategoryDto>> Post(CategoryAddUpdateDto categoryDto)
    {
        var result = await _categoryService.CreateCategoryAsync(categoryDto);

        if (result.IsSuccess)
            // Para 201 Created, es necesario mantener CreatedAtAction para enviar la URL del recurso creado
            return CreatedAtAction(nameof(Get), new { id = result.Data.Id },
                                    new ApiResponse<CategoryDto>(201, "Category created successfully.", result.Data));

        // Fallo: ToActionResult aplica el 409 o 500 del ServiceResult
        return result.ToActionResult();
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)] // Error de Service (Category not found)
    [ProducesResponseType(StatusCodes.Status409Conflict)] // Error de Service (Duplicate name)
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CategoryDto>> Put(int id, [FromBody] CategoryAddUpdateDto categoryDto)
    {
        var result = await _categoryService.UpdateCategoryAsync(id, categoryDto);

        if (result.IsSuccess)
            return Ok(new ApiResponse<CategoryDto>(200, "Category updated successfully.", result.Data));

        // Fallo: ToActionResult aplica el 404, 409 o 500 del ServiceResult
        return result.ToActionResult();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)] // Error de Service (Category not found)
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _categoryService.DeleteCategoryAsync(id);
        return result.ToActionResult();
    }
}
