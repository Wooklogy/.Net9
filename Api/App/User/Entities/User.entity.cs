using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Api.Infra.Base; // BaseEntityUuid 위치
using Share.Enums;    // EnumRole 위치

namespace Api.App.User.Entities;

[Table("user")]
[Index(nameof(Identify), IsUnique = true)]
[Comment("User Table for trading system")]
public class UserEntity : BaseEntityUuid
{
    [Required]
    [MaxLength(128)]
    [Comment("User login login id (Unique)")]
    public required string Identify { get; set; }

    [Required]
    [MaxLength(255)] // 해싱된 비밀번호는 길어질 수 있으므로 여유 있게 설정
    [Comment("Hashed password")]
    public required string Password { get; set; }

    [Required]
    [Comment("User access level (Admin, SubAdmin, User)")]
    // AppDbContext에서 HasConversion<string>() 처리했으므로 DB엔 문자열로 저장됩니다.
    public EnumRole Role { get; set; } = EnumRole.User;

    [Comment("The last time the user logged in")]
    public DateTime? LastLoginAt { get; set; }

    [Comment("Admin UUID who created this user")]
    public Guid? CreatedById { get; set; }

    [ForeignKey(nameof(CreatedById))]
    public virtual UserEntity? CreatedBy { get; set; } // 지연 로딩을 위해 virtual 권장
}