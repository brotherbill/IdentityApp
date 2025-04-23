﻿using System.Security.Claims;
using System.Threading.Tasks;
using Api.DTOs.Account;
using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

    [Authorize]
    [HttpGet("refresh-user-token")]
    public async Task<ActionResult<UserDto>> RefreshUserToken() {
        var user = await _userManager.FindByNameAsync(User.FindFirst(ClaimTypes.Email)?.Value);
        return CreateApplicationUserDto(user);
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

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto model) {
        if (await CheckEmailExistsAsync(model.Email)) {
            return BadRequest($"An existing account is using {model.Email}, email address.  Please try with another email address");
        }

        var userToAdd = new User {
            FirstName = model.FirstName.Trim().ToLower(),
            LastName = model.LastName.Trim().ToLower(),
            UserName = model.Email.Trim().ToLower(),
            Email = model.Email.Trim().ToLower(),
            EmailConfirmed = true, // For simplicity, we are confirming the email automatically.  Will fix this later.
        };

        var result = await _userManager.CreateAsync(userToAdd, model.Password);
        if (!result.Succeeded) return BadRequest(result.Errors);

        return Ok(new JsonResult(new { title = "Account Created", message = "Your account has been created, you can login" }));
    }

    #region Private Helper Methods

    private UserDto CreateApplicationUserDto(User user) {
        return new UserDto {
            FirstName = user.FirstName,
            LastName = user.LastName,
            JWT = _jwtService.CreateJWT(user),
        };
    }

    private async Task<bool> CheckEmailExistsAsync(string email) {
        return await _userManager.Users.AnyAsync(user => user.Email == email.ToLower());
    }
    #endregion

}

