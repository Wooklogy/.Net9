using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Api.Config.Monitoring;

public class ApiMetrics
{
    private readonly Counter<long> _requestCounter;
    private readonly Histogram<double> _requestDuration;

    public ApiMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("Performance");

        // 요청 횟수 카운터 (누적 수치)
        _requestCounter = meter.CreateCounter<long>("api.requests.total", description: "Total number of API requests");
        
        // 요청 처리 시간 히스토그램 (분포도 측정)
        _requestDuration = meter.CreateHistogram<double>("api.requests.duration", "ms", "Duration of API requests");
    }

    public void RecordRequest(string method, string path, int statusCode, double elapsedMs)
    {
        var tags = new TagList { { "method", method }, { "path", path }, { "status_code", statusCode } };
        
        _requestCounter.Add(1, tags);
        _requestDuration.Record(elapsedMs, tags);
    }
}