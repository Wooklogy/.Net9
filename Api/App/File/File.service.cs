using Amazon.S3;
using Amazon.S3.Transfer;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using Share.Enums;
using Share.Models;
using Api.App.File.Dto;
using Api.App.File.Entities;
using Api.App.File.Interfaces;
using Api.Infra.Base;

namespace Api.App.File;

public class FileService(
    IAmazonS3 s3Client,
    IFileRepository fileRepository,
    IOptions<BaseAWSS3Options> s3Options) : IFileService
{
    private readonly string _bucketName = s3Options.Value.BucketName;

    public async Task<FileEntity> UploadFileAsync(Stream fileStream, GetFileQTO fileInfo, EnumFileType type)
    {
        var newUuid = Guid.NewGuid();
        var extension = Path.GetExtension(fileInfo.OriginName);
        var s3Key = $"{newUuid}{extension}";

        // 1. S3 업로드 설정
        var uploadRequest = new TransferUtilityUploadRequest
        {
            InputStream = fileStream,
            Key = s3Key,
            BucketName = _bucketName,
            ContentType = fileInfo.ContentType,
            // Public 여부에 따른 ACL 설정
            CannedACL = type == EnumFileType.Public ? S3CannedACL.PublicRead : S3CannedACL.Private
        };

        var fileTransferUtility = new TransferUtility(s3Client);

        try
        {
            // S3 업로드 실행
            await fileTransferUtility.UploadAsync(uploadRequest);

            // 2. DB 엔티티 생성
            var fileUrl = type == EnumFileType.Public
                ? $"https://{_bucketName}.s3.amazonaws.com/{s3Key}"
                : string.Empty; // Private은 Presigned URL을 통해 제공하므로 기본 URL 비움 처리 가능

            var entity = new FileEntity
            {
                Name = s3Key,
                Extension = extension,
                OriginName = fileInfo.OriginName,
                Size = fileInfo.Size,
                Type = type,
                Url = fileUrl
            };

            return await fileRepository.CreateFileAsync(entity);
        }
        catch (Exception)
        {
            // DB 저장 실패 시 이미 업로드된 S3 객체 삭제 (롤백)
            await s3Client.DeleteObjectAsync(_bucketName, s3Key);
            throw;
        }
    }

    public async Task<string> GetDownloadUrlAsync(Guid fileUuid)
    {
        var file = await fileRepository.GetByUuidAsync(fileUuid);

        // Public 파일인 경우 저장된 URL 즉시 반환
        if (file.Type == EnumFileType.Public && !string.IsNullOrEmpty(file.Url))
            return file.Url;

        // Private 파일이거나 URL이 없는 경우 Presigned URL 생성
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = file.Name,
            Expires = DateTime.UtcNow.AddMinutes(30) // 만료 시간 30분 설정
        };

        return s3Client.GetPreSignedURL(request);
    }

    public async Task<BasePaginationSTO<FileEntity>> GetFileListAsync(GetFileSearchQTO dto)
    {
        // Repository의 페이징 로직 호출
        return await fileRepository.GetFileList(dto);
    }
}