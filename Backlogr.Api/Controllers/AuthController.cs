// /Controllers/AuthController.cs
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Backlogr.Api.Common;
using Backlogr.Api.DTOs.Auth;
using Backlogr.Api.Models.Entities;
using Backlogr.Api.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Backlogr.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly JwtOptions _jwtOptions;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<ApplicationRole> roleManager,
        IOptions<JwtOptions> jwtOptions)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _jwtOptions = jwtOptions.Value;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.UserName) ||
            string.IsNullOrWhiteSpace(dto.DisplayName) ||
            string.IsNullOrWhiteSpace(dto.Email) ||
            string.IsNullOrWhiteSpace(dto.Password))
        {
            return BadRequest("All fields are required.");
        }

        var existingUserByEmail = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUserByEmail is not null)
        {
            return BadRequest("An account with that email already exists.");
        }

        var existingUserByUserName = await _userManager.FindByNameAsync(dto.UserName);
        if (existingUserByUserName is not null)
        {
            return BadRequest("That username is already taken.");
        }

        if (!await _roleManager.RoleExistsAsync(RoleNames.User))
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                "Default role 'User' has not been configured.");
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = dto.UserName,
            Email = dto.Email,
            DisplayName = dto.DisplayName,
            CreatedAt = DateTime.UtcNow
        };

        var createResult = await _userManager.CreateAsync(user, dto.Password);
        if (!createResult.Succeeded)
        {
            return BadRequest(createResult.Errors.Select(error => error.Description));
        }

        var addToRoleResult = await _userManager.AddToRoleAsync(user, RoleNames.User);
        if (!addToRoleResult.Succeeded)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                addToRoleResult.Errors.Select(error => error.Description));
        }

        var roles = await _userManager.GetRolesAsync(user);
        var response = BuildAuthResponse(user, roles);

        return Ok(response);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.EmailOrUserName) ||
            string.IsNullOrWhiteSpace(dto.Password))
        {
            return BadRequest("Email or username and password are required.");
        }

        ApplicationUser? user;

        if (dto.EmailOrUserName.Contains('@'))
        {
            user = await _userManager.FindByEmailAsync(dto.EmailOrUserName);
        }
        else
        {
            user = await _userManager.FindByNameAsync(dto.EmailOrUserName);
        }

        if (user is null)
        {
            return Unauthorized("Invalid credentials.");
        }

        var passwordResult = await _signInManager.CheckPasswordSignInAsync(
            user,
            dto.Password,
            lockoutOnFailure: false);

        if (!passwordResult.Succeeded)
        {
            return Unauthorized("Invalid credentials.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var response = BuildAuthResponse(user, roles);

        return Ok(response);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<MeResponseDto>> Me()
    {
        var userId = GetCurrentUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var user = await _userManager.FindByIdAsync(userId.Value.ToString());
        if (user is null)
        {
            return Unauthorized();
        }

        var roles = await _userManager.GetRolesAsync(user);

        var response = new MeResponseDto
        {
            UserId = user.Id,
            UserName = user.UserName ?? string.Empty,
            DisplayName = user.DisplayName,
            Email = user.Email ?? string.Empty,
            AvatarUrl = user.AvatarUrl,
            Bio = user.Bio,
            Roles = roles
        };

        return Ok(response);
    }

    private Guid? GetCurrentUserId()
    {
        var userIdValue =
            User.FindFirstValue(ClaimTypes.NameIdentifier) ??
            User.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
            User.FindFirstValue("sub");

        if (Guid.TryParse(userIdValue, out var userId))
        {
            return userId;
        }

        return null;
    }

    private AuthResponseDto BuildAuthResponse(ApplicationUser user, IList<string> roles)
    {
        var expiresAtUtc = DateTime.UtcNow.AddDays(7);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName ?? string.Empty),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new("display_name", user.DisplayName)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);

        return new AuthResponseDto
        {
            Token = tokenValue,
            ExpiresAtUtc = expiresAtUtc,
            UserId = user.Id,
            UserName = user.UserName ?? string.Empty,
            DisplayName = user.DisplayName,
            Email = user.Email ?? string.Empty,
            Roles = roles
        };
    }
}