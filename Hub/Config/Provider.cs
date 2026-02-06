using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Hub.Config;

public class UuidUserIdProvider : IUserIdProvider
{
    /// <summary>
    /// 유저의 DB UUID를 SignalR의 고유 식별자로 지정합니다.
    /// </summary>
    public string? GetUserId(HubConnectionContext connection)
    {
        // JWT의 'sub' 클레임 또는 별도로 지정한 'uuid' 클레임에서 값을 추출합니다.
        // 보통 표준 JWT에서는 NameIdentifier(sub)에 UUID를 담습니다.
        return connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value 
               ?? connection.User?.FindFirst("uuid")?.Value;
    }
}