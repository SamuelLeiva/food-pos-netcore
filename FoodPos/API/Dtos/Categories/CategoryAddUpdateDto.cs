using System.ComponentModel.DataAnnotations;

namespace API.Dtos.Categories;

public class CategoryAddUpdateDto
{
    [Required(ErrorMessage = "Category's name is required.")]
    [MinLength(3, ErrorMessage = "Name must be at least 3 characters long.")]
    [MaxLength(500, ErrorMessage = "Name can't be longer than 500 characters.")]
    public string Name { get; set; }
}
