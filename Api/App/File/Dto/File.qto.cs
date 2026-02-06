using System.ComponentModel.DataAnnotations;
using Share.Enums;
using Share.Models;

namespace Api.App.File.Dto;

/// <summary>
/// Data transfer object for internal file processing and S3 metadata.
/// </summary>
public sealed class GetFileQTO
{
    /// <summary> The original name of the file including extension (e.g., "document.pdf"). </summary>
    public required string OriginName { get; set; }

    /// <summary> The unique stored name of the file in S3 (usually a UUID + extension). </summary>
    public required string Name { get; set; }

    /// <summary> The MIME type of the file (e.g., "image/png", "application/pdf"). </summary>
    public required string ContentType { get; set; }

    /// <summary> The total size of the file in bytes. </summary>
    public required int Size { get; set; }

    /// <summary> The public access URL of the file (only for public files). </summary>
    public string? Url { get; set; }
}

public class GetFileSearchQTO : BaseFilterQTO
{
    public string? OriginName { get; set; }

    public EnumFileType? Type { get; set; }

    public string? Name { get; set; }

    public string? Extension { get; set; }

    public long? SizeFrom { get; set; }

    public long? SizeTo { get; set; }
}

public sealed class GetDownloadFileQTO
{
    [Required]
    public required Guid Uuid { get; set; }
}