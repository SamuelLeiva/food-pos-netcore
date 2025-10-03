using API.Dtos.Products;
using API.Helpers;
using API.Helpers.Response;
using API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API.Extensions; // Asegúrate de que esta extensión ToActionResult() esté aquí

namespace API.Controllers
{
    public class ProductsController : BaseApiController
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        // 1. Obtener todos los productos paginados
        [HttpGet]
        [Authorize(Roles = "Admin,Client")] // Permitimos acceso a clientes también si es el catálogo principal
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Pager<ProductDto>>> Get([FromQuery] Params productParams)
        {
            var result = await _productService.GetProductsAsync(productParams);

            // Si es exitoso, devuelve 200 OK. Si falla, devuelve 500 (desde el service)
            return result.ToActionResult();
        }

        // 2. Obtener productos por categoría
        [HttpGet("category/{categoryId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Pager<ProductDto>>> Get(int categoryId, [FromQuery] Params productParams)
        {
            var result = await _productService.GetProductsByCategoryAsync(categoryId, productParams);

            // Si es exitoso, devuelve 200 OK. Si falla, devuelve el error del service (probablemente 500)
            return result.ToActionResult();
        }

        // 3. Obtener producto por ID
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ProductDto>> Get(int id)
        {
            var result = await _productService.GetProductByIdAsync(id);

            // Si falla, ToActionResult usará el 404 NotFound o el 500 del service.
            return result.ToActionResult();
        }

        // 4. Crear un nuevo producto
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ProductDto>> Post(ProductAddUpdateDto productDto)
        {
            var result = await _productService.CreateProductAsync(productDto);

            if (result.IsSuccess)
                // Usamos CreatedAtAction para el 201 RESTful
                return CreatedAtAction(nameof(Get), new { id = result.Data.Id },
                                        new ApiResponse<ProductDto>(201, "Product created successfully.", result.Data));

            // Si falla, ToActionResult usará el 409 Conflict o el 500 del service.
            return result.ToActionResult();
        }

        // 5. Actualizar un producto existente
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ProductDto>> Put(int id, [FromBody] ProductAddUpdateDto productDto)
        {
            var result = await _productService.UpdateProductAsync(id, productDto);

            if (result.IsSuccess)
                // Retorna 200 OK con el recurso actualizado en el cuerpo
                return Ok(new ApiResponse<ProductDto>(200, "Product updated successfully.", result.Data));

            // Si falla, ToActionResult usará el 404 Not Found, 409 Conflict o 500 del service.
            return result.ToActionResult();
        }

        // 6. Eliminar un producto
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _productService.DeleteProductAsync(id);

            if (result.IsSuccess)
                // Retorna 204 No Content para eliminación exitosa (sin cuerpo)
                return NoContent();

            // Si falla, ToActionResult usará el 404 Not Found (desde el service)
            return result.ToActionResult();
        }
    }
}