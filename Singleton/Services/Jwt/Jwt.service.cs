using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Share.Enums;
using Singleton.Services.Jwt.Interfaces;

namespace Singleton.Services.Jwt;


public sealed class JwtTokenService : IJwtTokenService
{
    private readonly SymmetricSecurityKey _signingKey;
    private readonly int _expireMinutes;
    private readonly string? _issuer;
    private readonly string? _audience;

    public JwtTokenService(IConfiguration config)
    {
        var jwt = config.GetSection("Jwt");
        var key = jwt["Key"] ?? throw new InvalidOperationException("Jwt:Key not configured.");

        _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        _expireMinutes = jwt.GetValue<int>("ExpireMinutes", 60);
        _issuer = jwt["Issuer"];
        _audience = jwt["Audience"];
    }

    public string GenerateToken(Guid userId, string identify, EnumRole role)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, identify),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, role.ToString())
        };

        var creds = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_expireMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}