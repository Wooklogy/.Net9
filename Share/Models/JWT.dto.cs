using Share.Enums;

namespace Share.Models;

public record JwtSTO(
    string AccessToken,
    string RefreshToken,
    long? ExpiresIn
);

public record UserConextDTO(Guid Uuid, string Identification, EnumRole? Role);