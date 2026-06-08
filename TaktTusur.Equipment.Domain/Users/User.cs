namespace TaktTusur.Equipment.Domain.Users;

public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = [];
}