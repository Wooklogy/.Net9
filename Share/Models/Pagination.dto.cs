using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Share.Models;


public abstract class BasePaginationQTO
{
    private const int DefaultSize = 10;
    private const int MaxSize = 100;

    /// <summary>현재 페이지 (1부터 시작)</summary>
    [DefaultValue(1)]
    public int Page { get; init; } = 1;

    /// <summary>페이지당 최대 콘텐츠 수</summary>
    [DefaultValue(DefaultSize)]
    public int Size { get; init; } = DefaultSize;

    [JsonIgnore]
    public int SafePage => Math.Max(1, Page);

    [JsonIgnore]
    public int SafeSize => Math.Clamp(Size, 1, MaxSize);

    [JsonIgnore]
    public int Skip => (SafePage - 1) * SafeSize;
}



/// <summary>페이징 응답 표준 객체</summary>
public class BasePaginationSTO<T>(
    int page,
    int size,
    int totalCount,
    IReadOnlyList<T> contents)
{
    public int Page { get; init; } = page;
    public int Size { get; init; } = size;
    public int TotalCount { get; init; } = totalCount;
    public int TotalPage { get; init; } = (int)Math.Ceiling(totalCount / (double)size);
    public bool HasNextPage { get; init; } = page < (int)Math.Ceiling(totalCount / (double)size);
    public bool HasPrevPage { get; init; } = page > 1;

    public IReadOnlyList<T> Contents { get; init; } = contents;
}