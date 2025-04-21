using System.Threading.Tasks;
using Api.DTOs.Account;
using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase {
    private readonly JWTService _jwtService;
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;

    public AccountController(
        JWTService jwtService, 
        SignInManager<User> signInManager, 
        UserManager<User> userManager) {
        _jwtService = jwtService;
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto model) {
        var user = await _userManager.FindByNameAsync(model.UserName);
        if (user is null) return Unauthorized("Invalid username or password");

        if (!user.EmailConfirmed) return Unauthorized("Please confirm your email.");

        var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
        if (!result.Succeeded) return Unauthorized("Invalid username or password");

        return Ok(CreateApplicationUserDto(user));
    }
    
    #region Private Helper Methods

    private UserDto CreateApplicationUserDto(User user) {
        return new UserDto {
            FirstName = user.FirstName,
            LastName = user.LastName,
            JWT = _jwtService.CreateJWT(user),
        };
    }
    #endregion

}

