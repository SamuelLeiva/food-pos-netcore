using Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace API.Dtos
{
    public class ProductAddUpdateDto
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Product's name is required.")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Product's description is required.")]
        public string Description { get; set; }
        public float Price { get; set; }
        public string ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public int CategoryId { get; set; }
    }

}
