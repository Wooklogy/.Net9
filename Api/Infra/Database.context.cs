using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Reflection;
using Api.Infra.Base;
using Api.App.User.Entities;
using Api.App.Auth.Entities;
using Api.App.File.Entities;

namespace Api.Infra;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    // Entities registering zone.
    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<PermissionEntity> Permissions => Set<PermissionEntity>();
    public DbSet<FileEntity> Files => Set<FileEntity>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. Ulid 변환기 정의
        var ulidConverter = new ValueConverter<Ulid, Guid>(
            v => v.ToGuid(),
            v => new Ulid(v)
        );

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // 2. Ulid 타입 속성에 컨버터 적용
            foreach (var property in entityType.GetProperties())
            {
                // [보완] 만약 엔티티에서 Id를 Guid로 선언했지만 
                // 실제 데이터 처리는 Ulid로 하고 싶다면 이 체크가 중요합니다.
                if (property.ClrType == typeof(Ulid) || property.ClrType == typeof(Ulid?))
                {
                    property.SetValueConverter(ulidConverter);
                }
            }

            // 3. 기존 Soft Delete 필터 적용 로직 (완벽합니다)
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(AppDbContext)
                    .GetMethod(nameof(ApplySoftDeleteFilter), BindingFlags.NonPublic | BindingFlags.Static)!
                    .MakeGenericMethod(entityType.ClrType);
                method.Invoke(null, [modelBuilder]);
            }
        }
    }
    private static void ApplySoftDeleteFilter<TEntity>(ModelBuilder builder) where TEntity : BaseEntity
    {
        builder.Entity<TEntity>().HasQueryFilter(e => e.DeletedAt == null);
    }

    // ---------------------------
    // Audit & Soft Delete Logic
    // ---------------------------
    public override int SaveChanges()
    {
        ApplyAuditRules();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditRules();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditRules()
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.UpdatedAt = now;
                    break;

                case EntityState.Modified:
                    entry.Property(e => e.CreatedAt).IsModified = false;
                    entry.Entity.UpdatedAt = now;
                    break;

                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Property(e => e.CreatedAt).IsModified = false;
                    entry.Entity.DeletedAt = now;
                    entry.Entity.UpdatedAt = now;
                    break;
            }
        }
    }

    public void HardRemove<TEntity>(TEntity entity) where TEntity : BaseEntity
    {
        base.Remove(entity);
    }
}