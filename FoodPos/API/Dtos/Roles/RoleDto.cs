using System.ComponentModel.DataAnnotations;

namespace API.Dtos.Roles;

public class RoleDto
{
    [Required(ErrorMessage = "Name can't be empty.")]
    [MaxLength(50, ErrorMessage = "Name can't be longer than 50 characters.")]
    public string Name { get; set; }
}
