using Microsoft.AspNetCore.Mvc;
using RealTime_Auctions.Models;
using RealTime_Auctions.Services;

namespace RealTime_Auctions.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;

    public UsersController(UserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] User user)
    {
        if (string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Password))
        {
            return BadRequest(new { message = "Korisnicko ime i lozinka su obavezni!" });
        }

        bool exists = await _userService.UserExistsAsync(user.Username);

        if (exists)
        {
            return Conflict(new { message = "Korisnik sa tim imenom vec postoji!" });
        }

        await _userService.CreateUserAsync(user);

        return Ok(new
        {
            message = "Uspesna registracija!",
            userId = user.Id
        });
    }
}