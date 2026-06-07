using System.ComponentModel.DataAnnotations;

namespace TaktTusur.Equipment.Api.Contracts.EquipmentRequests;

public class ApproveEquipmentRequestDto
{
    [MinLength(1)]
    public string? RequestText { get; set; }
}
