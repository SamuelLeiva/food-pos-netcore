using API.Dtos;
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
        public async Task<ActionResult<Product>> Post(ProductAddUpdateDto productDto)
        {
            var product = _mapper.Map<Product>(productDto);

            _unitOfWork.Products.Add(product);
            await _unitOfWork.SaveAsync();
            if (product == null)
            {
                return BadRequest(new ApiResponse(400));
            }

            //productDto.Id = product.Id;
            return CreatedAtAction(nameof(Post), new { id = product.Id }, productDto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProductAddUpdateDto>> Put(int id, [FromBody] ProductAddUpdateDto productDto)
        {
            if (productDto == null)
                return NotFound(new ApiResponse(404, "The product requested does not exist."));

            var productDb = await _unitOfWork.Products.GetByIdAsync(id);
            if (productDb == null)
                return NotFound(new ApiResponse(404, "The product requested does not exist."));

            //var product = _mapper.Map<Product>(productDto);
            _mapper.Map(productDto, productDb);

            //_unitOfWork.Products.Update(product);
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
