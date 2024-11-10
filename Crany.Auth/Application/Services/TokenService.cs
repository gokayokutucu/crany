using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Crany.Auth.Application.Services.Abstractions;
using Crany.Auth.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace Crany.Auth.Application.Services;

public class TokenService(UserManager<ApplicationUser> userManager, IConfiguration config) : ITokenService
{
    public string CreateToken(ApplicationUser user, IList<string> roles)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("uid", user.Id),
            new Claim("api_key", user.ApiKey ?? string.Empty)
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            config["Jwt:Issuer"],
            config["Jwt:Audience"],
            claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    public async Task<string> CreateAndStoreRefreshTokenAsync(ApplicationUser user)
    {
        var randomNumber = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
        }
        var refreshToken = Convert.ToBase64String(randomNumber);

        await userManager.SetAuthenticationTokenAsync(
            user,
            "Crany.Auth",
            "RefreshToken",
            refreshToken
        );

        return refreshToken;
    }
}