using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Api.Infra.Base;
using Share.Enums;
using Api.App.User.Entities;

namespace Api.App.Auth.Entities;

[Table("permission")]
// 유저 한 명에게 동일한 권한이 중복으로 들어가지 않도록 복합 인덱스(Unique) 설정 권장
[Index(nameof(TargetIsId), nameof(Permission), IsUnique = true)]
[Comment("Fine-grained permission management table")]
public class PermissionEntity : BaseEntityUlid
{
    [Required]
    [Comment("The specific permission type (e.g., Trade_Read, Admin_Write)")]
    public EnumPermission Permission { get; set; }

    [Comment("Target user who possesses this permission")]
    public Guid? TargetIsId { get; set; }

    [ForeignKey(nameof(TargetIsId))]
    public virtual UserEntity? TargetIs { get; set; }

    [Comment("Manager/Admin who granted this permission")]
    public Guid? CreatedById { get; set; }

    [ForeignKey(nameof(CreatedById))]
    public virtual UserEntity? CreatedBy { get; set; }
}