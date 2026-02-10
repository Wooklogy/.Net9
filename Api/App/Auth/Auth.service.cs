using Api.App.Auth.Dto;
using Api.App.Auth.Interfaces;
using Api.App.User.Entities;
using Api.Config.Error;
using Share.Enums;
using Share.Utils;
using Singleton.Services.Jwt.Interfaces;

namespace Api.App.Auth;

public class AuthService(IAuthRepository authRepository, IJwtTokenService jwt) : IAuthService
{
    public async Task SignUp(PostSignUpQTO qto)
    {
        if (await authRepository.ExistsByIdentifyAsync(qto.Identify))
            throw new ConflictException("exception_duplicate_user_id");

        var user = new UserEntity
        {
            Identify = qto.Identify,
            Password = ToolHash.Bcrypt(qto.Password),
            Role = EnumRole.User
        };

        await authRepository.AddAsync(user);
        await authRepository.SaveChangesAsync();
    }

    public async Task<string> Login(GetLoginQTO qto)
    {
        var user = await authRepository.GetByIdentifyAsync(qto.Identify)
                   ?? throw new BadRequestException("exception_notfound_user_infomation");

        if (!ToolHash.Verify(qto.Password, user.Password))
            throw new BadRequestException("exception_notfound_user_infomation");

        user.LastLoginAt = DateTime.UtcNow;
        await authRepository.SaveChangesAsync();

        return jwt.GenerateToken(user.Id, user.Identify, user.Role);
    }

}