using GlowBook.Model.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GlowBook.Web.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _config;

    public AuthController(UserManager<ApplicationUser> userManager, IConfiguration config)
    {
        _userManager = userManager;
        _config = config;
    }

    public record LoginRequest(string Email, string Password);
    public record LoginResponse(string AccessToken, DateTime ExpiresUtc, string Email, string DisplayName, string[] Roles, string[] Permissions);

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest req)
    {
        var user = await _userManager.FindByEmailAsync(req.Email);
        if (user == null)
            return Unauthorized("Onbekende gebruiker.");

        if (!user.IsActive)
            return Unauthorized("Gebruiker is gedeactiveerd.");

        var ok = await _userManager.CheckPasswordAsync(user, req.Password);
        if (!ok)
            return Unauthorized("Foute login.");

        var roles = (await _userManager.GetRolesAsync(user)).ToArray();
        var userClaims = await _userManager.GetClaimsAsync(user);

        var permissions = userClaims
            .Where(c => c.Type == "permission")
            .Select(c => c.Value)
            .Distinct()
            .ToArray();

        var jwtKey = _config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key ontbreekt");
        var issuer = _config["Jwt:Issuer"] ?? "GlowBook";
        var audience = _config["Jwt:Audience"] ?? "GlowBookMobile";
        var days = int.TryParse(_config["Jwt:DaysValid"], out var d) ? d : 30;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? req.Email),
            new(ClaimTypes.Name, user.UserName ?? req.Email),
        };

        foreach (var r in roles)
            claims.Add(new Claim(ClaimTypes.Role, r));

        foreach (var p in permissions)
            claims.Add(new Claim("permission", p));

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var expiresUtc = DateTime.UtcNow.AddDays(days);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresUtc,
            signingCredentials: creds);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new LoginResponse(
            AccessToken: tokenString,
            ExpiresUtc: expiresUtc,
            Email: user.Email ?? req.Email,
            DisplayName: user.DisplayName,
            Roles: roles,
            Permissions: permissions
        ));
    }
}
