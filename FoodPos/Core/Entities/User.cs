namespace Core.Entities;

public class User : BaseEntity
{
    public string Names { get; set; }
    public string FirstSurname { get; set; }
    public string LastSurname { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public ICollection<Role> Roles { get; set; } = new HashSet<Role>();
    public ICollection<UserRoles> UserRoles { get; set; }
}
