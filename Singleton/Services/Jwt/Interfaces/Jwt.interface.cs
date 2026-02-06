using Share.Enums;

namespace Singleton.Services.Jwt.Interfaces;


public interface IJwtTokenService
{
    string GenerateToken(Guid userId, string identify, EnumRole role);
}
