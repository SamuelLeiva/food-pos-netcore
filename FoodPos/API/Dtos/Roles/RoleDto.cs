using System.ComponentModel.DataAnnotations;

namespace API.Dtos.Roles;

public class RoleDto
{
    public int? Id { get; set; }
    [Required(ErrorMessage = "Name can't be empty.")]
    [MaxLength(50, ErrorMessage = "Name can't be longer than 50 characters.")]
    [RegularExpression(@"^[a-zA-Z0-9\s]+$", ErrorMessage = "Role name can only contain letters, numbers, and spaces.")]
    public string Name { get; set; }
}
