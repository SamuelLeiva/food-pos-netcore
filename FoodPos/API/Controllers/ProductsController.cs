using API.Dtos;
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
        public async Task<ActionResult<IEnumerable<ProductListDto>>> Get()
        {
            var products = await _unitOfWork.Products.GetAllAsync();
            return _mapper.Map<List<ProductListDto>>(products);
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
        public async Task<ActionResult<Product>> Post(Product product)
        {
            _unitOfWork.Products.Add(product);
            _unitOfWork.Save();
            if (product == null)
            {
                return BadRequest();
            }

            return CreatedAtAction(nameof(Post), new {id = product.Id}, product);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Product>> Put(int id, [FromBody] Product product)
        {
            if (product == null)
                return NotFound();

            _unitOfWork.Products.Update(product);
            _unitOfWork.Save();

            return product;
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
            _unitOfWork.Save();

            return NoContent();
        }

    }
}
