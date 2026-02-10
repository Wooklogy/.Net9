using Api.Infra;
using Microsoft.EntityFrameworkCore;
using Api.App.File.Entities;
using Api.App.File.Dto;
using Share.Models;
using Api.Config.Error;
using Api.App.File.Interfaces;

namespace Api.App.File;

public class FileRepository(AppDbContext db) : IFileRepository
{
    public async Task<FileEntity> CreateFileAsync(FileEntity entity)
    {
        db.Files.Add(entity);
        await db.SaveChangesAsync();
        return entity;
    }

    public async Task<FileEntity> GetByUuidAsync(Guid uuid)
    {
        return await db.Files.FirstOrDefaultAsync(f => f.Id == uuid)
               ?? throw new NotFoundException("Couldn't find the file");
    }

    public async Task<BasePaginationSTO<FileEntity>> GetFileList(GetFileSearchQTO dto)
    {
        var query = db.Files.AsNoTracking();

        // 필터링 로직
        if (!string.IsNullOrWhiteSpace(dto.OriginName))
            query = query.Where(f => f.OriginName!.Contains(dto.OriginName));

        if (!string.IsNullOrWhiteSpace(dto.Name))
            query = query.Where(f => f.Name == dto.Name);

        if (dto.Type.HasValue)
            query = query.Where(f => f.Type == dto.Type);

        if (dto.SizeFrom.HasValue)
            query = query.Where(x => x.Size >= dto.SizeFrom.Value);

        if (dto.SizeTo.HasValue)
            query = query.Where(x => x.Size <= dto.SizeTo.Value);

        var totalCount = await query.CountAsync();

        var contents = await query
            .OrderByDescending(f => f.CreatedAt)
            .Skip(dto.Skip)
            .Take(dto.SafeSize)
            .ToListAsync();

        return new BasePaginationSTO<FileEntity>(dto.SafePage, dto.SafeSize, totalCount, contents);
    }
}