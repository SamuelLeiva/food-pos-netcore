using Core.Entities;

namespace API.Dtos
{
    public class ProductDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public float Price { get; set; }
        public string ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public CategoryDto Category { get; set; }
    }
}
