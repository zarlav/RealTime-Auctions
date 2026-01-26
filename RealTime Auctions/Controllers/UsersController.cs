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
            return BadRequest(new { message = "Korisničko ime i lozinka su obavezni!" });
        }

        await _userService.CreateUserAsync(user);

        return Ok(new
        {
            message = "Uspešna registracija!",
            userId = user.Id
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok("Lista korisnika je u Redisu.");
    }
}