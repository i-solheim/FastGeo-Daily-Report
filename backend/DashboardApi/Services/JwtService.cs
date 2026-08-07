using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using DashboardApi.Models;

namespace DashboardApi.Services;

public class JwtService
{
    private readonly string _secret;
    private readonly string _issuer;
    private readonly string _audience;
    public JwtService(IConfiguration configuration)
    {
        _secret = configuration["Jwt:Secret"]!;
        _issuer = configuration["Jwt:Issuer"]!;
        _audience = configuration["Jwt:Audience"]!;
    }

    public string CreateToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim("display_name", user.DisplayName),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var encodedKey = Encoding.UTF8.GetBytes(_secret);

        var key = new SymmetricSecurityKey(encodedKey);

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(1),
            signingCredentials: credentials);

        var handler = new JwtSecurityTokenHandler();

        return handler.WriteToken(token);
    }
}