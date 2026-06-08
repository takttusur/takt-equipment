using Microsoft.AspNetCore.Mvc;
using TaktTusur.Equipment.Api.Services.Users;
using TaktTusur.Equipment.Domain.Users;

namespace TaktTusur.Equipment.Api.Controllers;

[ApiController]
[Route("api/user")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("current")]
    public Task<User> GetCurrentUserAsync()
    {
        return _userService.GetCurrentUserAsync();
    }
}
