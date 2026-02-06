using System.ComponentModel.DataAnnotations;
using Share.Enums;
using Share.Models;
using Share.Constants;

namespace Api.App.Auth.Dto;


public sealed class PostSignUpQTO
{
    [Required]
    [RegularExpression(RegexConstants.IdentifyRegex, ErrorMessage = "validation_identify")]
    public required string Identify { get; set; }

    [Required]
    [RegularExpression(RegexConstants.PasswordRegex, ErrorMessage = "validation_password")]
    public required string Password { get; set; }
}


public sealed class GetLoginQTO
{
    [Required]
    public required string Identify { get; set; }
    [Required]
    public required string Password { get; set; }
}


public sealed class GetAllUsersQTO : BasePaginationQTO
{
    public string? Keyword { get; set; }
}

public sealed class GetAllUserSTO
{
    public required Guid Uuid { get; set; }
    public required string Identify { get; set; }
    public EnumRole? Role { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public required DateTime CreatedAt { get; set; }
}
