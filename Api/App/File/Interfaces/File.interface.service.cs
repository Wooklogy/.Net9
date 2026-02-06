using Api.App.File.Dto;
using Api.App.File.Entities;
using Share.Enums;
using Share.Models;

namespace Api.App.File.Interfaces;

public interface IFileService
{
    /// <summary>S3 업로드 및 DB 기록 조율</summary>
    Task<FileEntity> UploadFileAsync(Stream fileStream, GetFileQTO fileInfo, EnumFileType type);

    /// <summary>다운로드용 Presigned URL 생성</summary>
    Task<string> GetDownloadUrlAsync(Guid fileUuid);

    /// <summary>파일 목록 페이징 조회</summary>
    Task<BasePaginationSTO<FileEntity>> GetFileListAsync(GetFileSearchQTO dto);
}