using Api.App.User.Entities;

namespace Api.App.Auth.Interfaces;

public interface IAuthRepository
{
    Task<UserEntity?> GetByIdentifyAsync(string identify);
    Task<bool> ExistsByIdentifyAsync(string identify);
    Task<int> GetTotalCountAsync(string? keyword);
    Task AddAsync(UserEntity user);
    Task SaveChangesAsync();
}