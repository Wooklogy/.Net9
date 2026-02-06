using Api.App.File.Dto;
using Api.App.File.Entities;
using Share.Models;

namespace Api.App.File.Interfaces;

public interface IFileRepository
{
    /// <summary>S3 파일 생성</summary>
    Task<FileEntity> CreateFileAsync(FileEntity entity);

    /// <summary>단일 파일 조회 (다운로드 URL 생성 등을 위한 기초 데이터 확보)</summary>
    Task<FileEntity> GetByUuidAsync(Guid fileUuid);

    /// <summary>목록 조회</summary>
    Task<BasePaginationSTO<FileEntity>> GetFileList(GetFileSearchQTO dto);
}