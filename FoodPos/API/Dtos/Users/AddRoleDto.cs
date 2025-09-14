using System.ComponentModel.DataAnnotations;

namespace API.Dtos.Users;

public class AddRoleDto
{
    [Required]
    public string UserName { get; set; }
    [Required]
    public string Password { get; set; }
    [Required]
    public string Role { get; set; }
}
    
