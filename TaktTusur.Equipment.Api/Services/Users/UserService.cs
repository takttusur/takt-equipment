using TaktTusur.Equipment.Domain.Users;

namespace TaktTusur.Equipment.Api.Services.Users;

public class UserService : IUserService
{
    private static readonly IReadOnlyList<User> Users =
    [
        new User
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Username = "current.user",
            Roles = ["Requester", "Issuer"]
        }
    ];

    public Task<User> GetCurrentUserAsync()
    {
        return Task.FromResult(Users[0]);
    }

    public Task<User> GetUserByIdAsync(Guid userId)
    {
        var user = Users.FirstOrDefault(x => x.Id == userId)
            ?? new User
            {
                Id = userId,
                Username = $"user-{userId:N}",
                Roles = []
            };

        return Task.FromResult(user);
    }
}
