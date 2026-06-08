namespace TaktTusur.Equipment.Api.Services.Users;

public class UserRolesService : IUserRolesService
{
    private readonly IUserService _userService;

    public UserRolesService(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<bool> CanRequestEquipmentAsync(Guid userId)
    {
        var user = await _userService.GetUserByIdAsync(userId);

        return user.Roles.Contains("Requester", StringComparer.OrdinalIgnoreCase)
            || user.Roles.Contains("Issuer", StringComparer.OrdinalIgnoreCase);
    }

    public async Task<bool> CanIssueEquipmentAsync(Guid userId)
    {
        var user = await _userService.GetUserByIdAsync(userId);

        return user.Roles.Contains("Issuer", StringComparer.OrdinalIgnoreCase);
    }
}
