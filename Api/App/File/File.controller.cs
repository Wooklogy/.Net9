using Api.App.File.Dto;
using Api.App.File.Entities;
using Api.App.File.Interfaces;
using Api.Config.Authorization;
using Api.Config.Error;
using Microsoft.AspNetCore.Mvc;
using Share.Enums;
using Share.Models;

namespace Api.App.File;

[ApiController]
[Route("file")]
[Tags("File")]
public class FileController(IFileService fileService) : ControllerBase
{
    [HttpPost("upload")]
    [AuthorizePermission(roles: [EnumRole.Admin, EnumRole.SubAdmin, EnumRole.User], permissions: [EnumPermission.File_Upload])]
    [EndpointSummary("Single File Upload")]
    [ProducesResponseType(typeof(FileEntity), StatusCodes.Status200OK)]
    [ProducesErrorCodes(400, 404)]
    public async Task<IActionResult> PrivateUpload(IFormFile file)
    {
        var result = await ProcessUploadAsync(file, EnumFileType.Private);
        return Ok(result);
    }

    [HttpPost("uploads")]
    [AuthorizePermission(roles: [EnumRole.Admin, EnumRole.SubAdmin, EnumRole.User], permissions: [EnumPermission.File_Upload])]
    [EndpointSummary("Bulk File Upload")]
    [ProducesResponseType(typeof(List<object>), StatusCodes.Status200OK)]
    [ProducesErrorCodes(400, 404)]
    public async Task<IActionResult> PrivateUploadMultiple(List<IFormFile> files)
    {
        var results = await ProcessMultipleUploadAsync(files, EnumFileType.Private);
        return Ok(results);
    }

    [HttpGet("download")]
    [AuthorizePermission(roles: [EnumRole.Admin, EnumRole.SubAdmin, EnumRole.User], permissions: [EnumPermission.File_Download])]
    [EndpointSummary("Generate Download URL")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesErrorCodes(400, 404)]
    public async Task<IActionResult> DownloadFile([FromQuery] GetDownloadFileQTO dto)
    {
        var url = await fileService.GetDownloadUrlAsync(dto.Uuid);
        return Ok(new { Url = url });
    }

    [HttpGet("list")]
    [AuthorizePermission(roles: [EnumRole.Admin, EnumRole.SubAdmin, EnumRole.User], permissions: [EnumPermission.File_Read])]
    [EndpointSummary("Get File Metadata List")]
    [EndpointDescription("Retrieves a paginated list of file metadata stored in the database.")]
    [ProducesResponseType(typeof(BasePaginationSTO<FileEntity>), StatusCodes.Status200OK)]
    [ProducesErrorCodes(400, 404)]
    public async Task<IActionResult> GetFileList([FromQuery] GetFileSearchQTO dto)
    {
        var files = await fileService.GetFileListAsync(dto);
        return Ok(files);
    }

    #region Private Helpers

    private async Task<FileEntity> ProcessUploadAsync(IFormFile file, EnumFileType type)
    {
        if (file == null || file.Length == 0)
            throw new BadRequestException("File validation failed: The uploaded file is empty or null.");

        using var stream = file.OpenReadStream();
        var fileQTO = new GetFileQTO
        {
            ContentType = file.ContentType,
            Name = Path.GetFileNameWithoutExtension(file.FileName),
            OriginName = file.FileName,
            Size = (int)file.Length
        };

        return await fileService.UploadFileAsync(stream, fileQTO, type);
    }

    private async Task<List<object>> ProcessMultipleUploadAsync(List<IFormFile> files, EnumFileType type)
    {
        if (files == null || files.Count == 0)
            throw new BadRequestException("File validation failed: No files were provided for upload.");

        var uploadedFiles = new List<object>();
        foreach (var file in files)
        {
            if (file.Length > 0)
            {
                var result = await ProcessUploadAsync(file, type);
                uploadedFiles.Add(new
                {
                    result.OriginName,
                    result.Url,
                    StoredName = result.Name
                });
            }
        }
        return uploadedFiles;
    }

    #endregion
}