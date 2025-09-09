using API.Services;

namespace API.Controllers;

public class UsersController : BaseApiController
{
    private readonly IUserService _userService; 
    public UsersController(IUserService userService)
    {
        _userService = userService;
    }
}
