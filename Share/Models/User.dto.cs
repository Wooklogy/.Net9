using Share.Enums;

namespace Share.Models;

public record UserCacheSTO(
    Guid UserId,
    EnumRole Role,
    List<EnumPermission> Permissions
);