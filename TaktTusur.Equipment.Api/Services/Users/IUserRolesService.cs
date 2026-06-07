namespace TaktTusur.Equipment.Api.Services.Users;

public interface IUserRolesService
{
    Task<bool> CanRequestEquipmentAsync(Guid userId);
    Task<bool> CanIssueEquipmentAsync(Guid userId);
}