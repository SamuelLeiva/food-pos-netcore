using API.Dtos.Products;
using API.Helpers;
using API.Helpers.Errors;
using API.Services.Interfaces;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Infrastructure.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    // [Authorize(Roles ="Admin")]
    public class ProductsController : BaseApiController
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        // Obtener productos paginados
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Pager<ProductDto>>> Get([FromQuery] Params productParams)
        {
            var result = await _productService.GetProductsAsync(productParams);
            if (result.IsSuccess)
                return Ok(result.Data);

            return BadRequest(new ApiResponse(400, result.ErrorMessage));
        }

        // Obtener productos por categoría
        [HttpGet("category/{categoryId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Pager<ProductDto>>> Get(int categoryId, [FromQuery] Params productParams)
        {
            var result = await _productService.GetProductsByCategoryAsync(categoryId, productParams);
            if (result.IsSuccess)
                return Ok(result.Data);

            return BadRequest(new ApiResponse(400, result.ErrorMessage));
        }

        // Obtener producto por ID
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductDto>> Get(int id)
        {
            var result = await _productService.GetProductByIdAsync(id);
            if (result.IsSuccess)
                return Ok(result.Data);

            return NotFound(new ApiResponse(404, result.ErrorMessage));
        }

        // Crear un nuevo producto
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ProductDto>> Post(ProductAddUpdateDto productDto)
        {
            var result = await _productService.CreateProductAsync(productDto);
            if (result.IsSuccess)
                return CreatedAtAction(nameof(Get), new { id = result.Data.Id }, result.Data);

            return Conflict(new ApiResponse(409, result.ErrorMessage));
        }

        // Actualizar un producto existente
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ProductDto>> Put(int id, [FromBody] ProductAddUpdateDto productDto)
        {
            var result = await _productService.UpdateProductAsync(id, productDto);

            if (result.IsSuccess)
                return Ok(result.Data);

            // Dependiendo del mensaje de error, retorna 404 o 409
            if (result.ErrorMessage.Contains("does not exist"))
            {
                return NotFound(new ApiResponse(404, result.ErrorMessage));
            }
            return Conflict(new ApiResponse(409, result.ErrorMessage));
        }

        // Eliminar un producto
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _productService.DeleteProductAsync(id);
            if (result.IsSuccess)
                return NoContent();

            return NotFound(new ApiResponse(404, result.ErrorMessage));     
        }

    }
}