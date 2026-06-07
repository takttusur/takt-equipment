namespace TaktTusur.Equipment.Domain.EquipmentRequest;

public class EquipmentRequest
{
    public Guid Id { get; set; }
    public string RequestText { get; set; } = string.Empty;
    public DateTime DateOfIssue { get; set; }
    public DateTime? DateOfReturn { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public EquipmentRequestStatus Status { get; set; }
    public Guid UserId { get; set; }
    public Guid? IssuedByUserId { get; set; }
    public Guid? ClosedByUserId { get; set; }
}