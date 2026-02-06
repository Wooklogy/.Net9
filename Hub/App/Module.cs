
using Hub.App.News;

namespace Hub.App;

public static class HubRegistration
{
    /// <summary>
    /// 모든 SignalR 허브 엔드포인트를 일괄 등록합니다. (Binance Style)
    /// </summary>
    public static void MapSystemHubs(this IEndpointRouteBuilder endpoints)
    {
        // 1. 자산 및 거래 관련 (보안/개인화 중심)
        endpoints.MapHub<NewsHub>("/hubs/news").RequireAuthorization();

        // 2. 시세 및 뉴스 관련 (고빈도/공용 데이터 중심)
        // endpoints.MapHub<MarketHub>("/hubs/market");

        // 3. 시스템 공지 및 알림 관련 (범용성 중심)
        // endpoints.MapHub<CommonHub>("/hubs/common");

        // 💡 팁: 나중에 특정 허브에만 별도의 미들웨어나 인증을 걸고 싶을 때 여기서 제어 가능합니다.
        // endpoints.MapHub<SpecialHub>("/hubs/special").RequireAuthorization("AdminOnly");
    }
}