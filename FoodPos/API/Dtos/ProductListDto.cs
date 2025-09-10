namespace API.Dtos
{
    public class ProductListDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public float Price { get; set; }
        public string ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public int CategoryId { get; set; }
        public string Category { get; set; }

        public ProductListDto()
        {

        }
    }
}
