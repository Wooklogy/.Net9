using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Infra.Base;

public abstract class BaseEntity
{
    // 거래소의 정밀한 시간 관리를 위해 DateTimeOffset 추천
    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; internal set; } = DateTimeOffset.UtcNow;

    [Column("updated_at")]
    public DateTimeOffset? UpdatedAt { get; internal set; }

    [Column("deleted_at")]
    public DateTimeOffset? DeletedAt { get; internal set; }

    public void Restore()
    {
        DeletedAt = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkAsUpdated() => UpdatedAt = DateTimeOffset.UtcNow;
    public void SoftDelete() => DeletedAt = DateTimeOffset.UtcNow;
}

// 1. 내부 관리용 (BIGINT)
public abstract class BaseEntityId : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public long Id { get; protected set; }
}

// 2. 외부 노출 및 정렬용 (ULID) - DB에는 Binary(16)/Guid로 저장하여 성능 극대화
public abstract class BaseEntityUlid : BaseEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; protected set; } = Ulid.NewUlid().ToGuid();

    // 도움말: 외부 출력 시에는 다시 Ulid로 변환해서 .ToString() 하면 됩니다.
    [NotMapped]
    public Ulid Ulid => new(Id);

    [NotMapped]
    public string DisplayId => Id.ToString()[^8..].ToUpper();
}


public abstract class BaseSTO
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public DateTimeOffset? DeletedAt { get; set; }
}