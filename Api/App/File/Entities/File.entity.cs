using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Api.Infra.Base;
using Share.Enums;

namespace Api.App.File.Entities;

[Table("file")]
[Comment("Metadata table for uploaded files (S3, etc.)")]
public class FileEntity : BaseEntityUuid
{
    [Required]
    [MaxLength(512)]
    [Comment("Publicly accessible URL")]
    public required string Url { get; set; }

    [Required]
    [Comment("Storage visibility type (Public, Private, etc.)")]
    public EnumFileType Type { get; set; } = EnumFileType.Public;

    [MaxLength(255)]
    [Comment("Storage-aliased filename")]
    public string? Name { get; set; }

    [MaxLength(255)]
    [Comment("Original filename from client")]
    public string? OriginName { get; set; }

    [MaxLength(50)]
    [Comment("File extension or MimeType")]
    public string? Extension { get; set; }

    [Comment("File size in bytes")]
    public long? Size { get; set; }
}