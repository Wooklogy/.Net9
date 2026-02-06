using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Api.Infra.Base;
using Api.App.User.Entities;
using Api.App.Auth.Entities;
using Api.App.File.Entities; 

namespace Api.Infra;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    // Entities resgisting zone.
    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<PermissionEntity> Permissions => Set<PermissionEntity>();
    public DbSet<FileEntity> Files => Set<FileEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. Soft Delete Query Filter (형님의 기존 로직 최적화)
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(AppDbContext)
                    .GetMethod(nameof(ApplySoftDeleteFilter), BindingFlags.NonPublic | BindingFlags.Static)!
                    .MakeGenericMethod(entityType.ClrType);
                method.Invoke(null, new object[] { modelBuilder });
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
        // 2만 명의 요청이 몰릴 때 DateTime.UtcNow는 루프 밖에서 고정하는 것이 일관성에 유리합니다.
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
                    // Soft Delete 구현: State를 Modified로 바꾸고 삭제 시간 기록
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