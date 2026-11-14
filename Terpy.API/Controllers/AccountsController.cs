using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Terpy.API.Helpers;
using Terpy.Shared.DTOs;
using Terpy.Shared.Entities;

namespace Terpy.API.Controllers;

[ApiController]
[Route("api/accounts")]
public class AccountsController : ControllerBase
{
    private readonly IUserHelper _userHelper;
    private readonly IConfiguration _configuration;

    public AccountsController(IUserHelper userHelper, IConfiguration configuration)
    {
        _userHelper = userHelper;
        _configuration = configuration;
    }

    [AllowAnonymous]
    [HttpPost("Login")]
    public async Task<ActionResult<TokenDTO>> Login(LoginDTO model)
    {
        var result = await _userHelper.LoginAsync(model);
        if (!result.Succeeded)
        {
            return BadRequest("Email o contraseña incorrectos.");
        }

        var user = await _userHelper.GetUserAsync(model.Email.Trim());
        if (user == null)
        {
            return BadRequest("Email o contraseña incorrectos.");
        }

        return Ok(await BuildTokenAsync(user));
    }

    private async Task<TokenDTO> BuildTokenAsync(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.Email!)
        };

        var roles = await _userHelper.GetRolesAsync(user);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["jwtKey"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiration = DateTime.UtcNow.AddDays(30);
        var token = new JwtSecurityToken(
            claims: claims,
            expires: expiration,
            signingCredentials: credentials);

        return new TokenDTO
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Expiration = expiration
        };
    }
}
