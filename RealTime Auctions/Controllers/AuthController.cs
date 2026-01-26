using Microsoft.AspNetCore.Mvc;
using RealTime_Auctions.Services;

namespace RealTime_Auctions.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserService _userService;

    public AuthController(UserService userService)
    {
        _userService = userService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userService.ValidateUserAsync(request.Username, request.Password);

        if (user == null)
        {
            return Unauthorized(new { message = "Pogrešno korisničko ime ili lozinka!" });
        }

        return Ok(new
        {
            id = user.Id,
            username = user.Username,
            email = user.Email
        });
    }
}

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}