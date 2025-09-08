using API.Dtos;
using API.Helpers;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
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
            var result = await _unitOfWork.Products.GetAllAsync(productParams.PageIndex, productParams.PageSize);
            var productsListDto = _mapper.Map<List<ProductListDto>>(result.registers);
            return new Pager<ProductListDto>(productsListDto, result.totalRegisters, productParams.PageIndex, productParams.PageSize);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductDto>> Get(int id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
                return NotFound();

            return _mapper.Map<ProductDto>(product);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Product>> Post(ProductAddUpdateDto productDto)
        {
            var product = _mapper.Map<Product>(productDto);

            _unitOfWork.Products.Add(product);
            await _unitOfWork.SaveAsync();
            if (product == null)
            {
                return BadRequest();
            }
            productDto.Id = product.Id;
            return CreatedAtAction(nameof(Post), new { id = productDto.Id }, productDto);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProductAddUpdateDto>> Put(int id, [FromBody] ProductAddUpdateDto productDto)
        {
            if (productDto == null)
                return NotFound();

            var product = _mapper.Map<Product>(productDto);

            _unitOfWork.Products.Update(product);
            await _unitOfWork.SaveAsync();

            return productDto;
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
                return NotFound();

            _unitOfWork.Products.Remove(product);
            await _unitOfWork.SaveAsync();

            return NoContent();
        }

    }
}
