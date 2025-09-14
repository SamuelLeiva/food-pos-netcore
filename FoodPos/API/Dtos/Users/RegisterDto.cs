using System.ComponentModel.DataAnnotations;

namespace API.Dtos.Users;

public class RegisterDto
{
    [Required]
    public string Names { get; set; }
    [Required]
    public string FirstSurname { get; set; }
    [Required]
    public string LastSurname { get; set; }
    [Required]
    public string Email { get; set; }
    [Required]
    public string UserName { get; set; }
    [Required]
    public string Password { get; set; }
}
