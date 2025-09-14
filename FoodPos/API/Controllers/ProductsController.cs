using API.Dtos.Products;
using API.Helpers;
using API.Helpers.Errors;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    // [Authorize(Roles ="Admin")]
    public class ProductsController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProductsController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Pager<ProductListDto>>> Get([FromQuery] Params productParams)
        {
            var result = await _unitOfWork.Products.GetAllAsync(productParams.PageIndex, productParams.PageSize, productParams.Search);
            var productsListDto = _mapper.Map<List<ProductListDto>>(result.registers);
            return new Pager<ProductListDto>(productsListDto, result.totalRegisters, productParams.PageIndex, productParams.PageSize, productParams.Search);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductDto>> Get(int id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
                return NotFound(new ApiResponse(404, "The product requested does not exist."));

            return _mapper.Map<ProductDto>(product);
        }
        
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<Product>> Post(ProductAddUpdateDto productDto)
        {
            var product = _mapper.Map<Product>(productDto);
            if (product == null)
            {
                return BadRequest(new ApiResponse(400));
            }

            var productExists = _unitOfWork.Products
                                    .Find(p => p.Name.ToLower() == productDto.Name.ToLower())
                                    .FirstOrDefault();

            if (productExists != null)
            {
                _unitOfWork.Products.Add(product);
                await _unitOfWork.SaveAsync();
                return CreatedAtAction(nameof(Post), new { id = product.Id }, productDto);
            } else
            {
                return Conflict(new ApiResponse(409, "A product with the same name already exists."));
            }            
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ProductAddUpdateDto>> Put(int id, [FromBody] ProductAddUpdateDto productDto)
        {
            if (productDto == null)
                return NotFound(new ApiResponse(404, "The product requested does not exist."));

            var productDb = await _unitOfWork.Products.GetByIdAsync(id);
            if (productDb == null)
                return NotFound(new ApiResponse(404, "The product requested does not exist."));

            var productExists = _unitOfWork.Products
                                    .Find(p => p.Name.ToLower() == productDto.Name.ToLower() && p.Id != id)
                                    .FirstOrDefault();

            if(productExists != null)
                return Conflict(new ApiResponse(409, "Another product with the same name already exists."));

            _mapper.Map(productDto, productDb);

            productDb.UpdatedAt = DateTime.Now;
            await _unitOfWork.SaveAsync();

            return productDto;
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
                return NotFound(new ApiResponse(404, "The product requested does not exist."));

            _unitOfWork.Products.Remove(product);
            await _unitOfWork.SaveAsync();

            return NoContent();
        }

    }
}
