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

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Pager<ProductListDto>>> Get([FromQuery] Params productParams)
        {
            var result = await _productService.GetProductsAsync(productParams);
            return Ok(result);
        }

        [HttpGet("category/{categoryId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Pager<ProductListDto>>> Get(int categoryId, [FromQuery] Params productParams)
        {
            var result = await _productService.GetProductsByCategoryAsync(categoryId, productParams);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductDto>> Get(int id)
        {
            var result = await _productService.GetProductByIdAsync(id);
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
        public async Task<ActionResult<Product>> Post(ProductAddUpdateDto productDto)
        {
            var result = await _productService.CreateProductAsync(productDto);
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
        public async Task<ActionResult<ProductDto>> Put(int id, [FromBody] ProductAddUpdateDto productDto)
        {
            var result = await _productService.UpdateProductAsync(id, productDto);
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
            var result = await _productService.DeleteProductAsync(id);
            if (!result.IsSuccess)
            {
                return NotFound(new ApiResponse(404, result.ErrorMessage));
            }

            return NoContent();
        }

    }
}
