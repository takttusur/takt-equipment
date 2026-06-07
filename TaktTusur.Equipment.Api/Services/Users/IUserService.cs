using TaktTusur.Equipment.Domain.Users;

namespace TaktTusur.Equipment.Api.Services.Users;

public interface IUserService
{
    Task<User> GetUserByIdAsync(Guid userId);
    Task<User> GetCurrentUserAsync();
}