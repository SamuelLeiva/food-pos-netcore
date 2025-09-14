using Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace API.Dtos.Products
{
    public class ProductAddUpdateDto
    {
        [Required(ErrorMessage = "Product's name is required.")]
        [MinLength(3, ErrorMessage = "Name must be at least 3 characters long.")]
        [MaxLength(500, ErrorMessage = "Name can't be longer than 500 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Product's description is required.")]
        [MinLength(10, ErrorMessage = "Description must be at least 10 characters long.")]
        [MaxLength(2000, ErrorMessage = "Description can't be longer than 2000 characters.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, 100000.00, ErrorMessage = "Price must be between $0.01 and $100,000.00.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Product's image URL is required.")]
        [Url(ErrorMessage = "The URL format is invalid.")]
        [MaxLength(500, ErrorMessage = "Image URL can't be longer than 500 characters.")]
        public string ImageUrl { get; set; }

        [Required(ErrorMessage = "IsActive must have a value.")]
        public bool IsActive { get; set; }

        [Required(ErrorMessage = "CategoryId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "CategoryId must be a positive number.")]
        public int CategoryId { get; set; }
    }

}
