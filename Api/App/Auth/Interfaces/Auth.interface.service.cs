using Api.App.Auth.Dto;

namespace Api.App.Auth.Interfaces;

public interface IAuthService
{
    Task SignUp(PostSignUpQTO qto);
    Task<string> Login(GetLoginQTO qto);
}