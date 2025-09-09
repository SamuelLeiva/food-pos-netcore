namespace API.Helpers;

public class Authorization
{
    public enum Roles
    {
        Admin,
        Client
    }

    public const Roles default_role = Roles.Client;
}
