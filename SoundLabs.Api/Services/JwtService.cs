using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SoundLabs.Models;

namespace SoundLabs.Services;

public class JwtService(IConfiguration config)
{
    public (string Token, DateTime ExpiresAt) Create(User user, string roleName)
    {
        var section = config.GetSection("Jwt");
        var expires = DateTime.UtcNow.AddHours(double.Parse(section["ExpireHours"] ?? "8"));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.IdUser.ToString()),
            new Claim(ClaimTypes.Name, user.Login),
            new Claim(ClaimTypes.Role, roleName)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(section["Key"]!));
        var token = new JwtSecurityToken(
            issuer: section["Issuer"],
            audience: section["Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}
