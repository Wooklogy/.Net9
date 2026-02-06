using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Infra.Base;

public abstract class BaseEntity
{
    // EF Core의 ApplyAuditRules에서 값을 수정할 수 있도록 internal set으로 변경합니다.
    [Column("created_at")]
    public DateTime CreatedAt { get; internal set; } 

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; internal set; }

    [Column("deleted_at")]
    public DateTime? DeletedAt { get; internal set; } // Soft Delete 필드

    protected BaseEntity()
    {
        // 초기값은 생성 시점 기준으로 설정
        var now = DateTime.UtcNow;
        CreatedAt = now;
    }

    public void Restore()
    {
        DeletedAt = null;
        UpdatedAt = DateTime.UtcNow;
    }
}

public abstract class BaseEntityId : BaseEntity 
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; private set; }
}

public abstract class BaseEntityUuid : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)] // 애플리케이션에서 생성 제어
    [Column("uuid")]
    public Guid Uuid { get; private set; } = Guid.NewGuid();
}