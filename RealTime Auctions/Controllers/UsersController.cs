using Microsoft.AspNetCore.Mvc;
using RealTime_Auctions.DTOs;
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
    public async Task<IActionResult> Register([FromBody] UserRegisterDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
        {
            return BadRequest(new { message = "Korisnicko ime i lozinka su obavezni!" });
        }
        bool exists = await _userService.UserExistsAsync(dto.Username);
        if (exists)
        {
            return Conflict(new { message = "Korisnik sa tim imenom vec postoji!" });
        }
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Username = dto.Username.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };

        await _userService.CreateUserAsync(user);

        return Ok(new
        {
            message = "Uspesna registracija!",
            userId = user.Id
        });
    }

}