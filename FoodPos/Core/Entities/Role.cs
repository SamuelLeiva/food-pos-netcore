
namespace Core.Entities;

public class Role : BaseEntity
{
    public string Name { get; set; }
    public ICollection<User> Users { get; set; } = new HashSet<User>();
    public ICollection<UserRoles> UserRoles { get; set; }
}
