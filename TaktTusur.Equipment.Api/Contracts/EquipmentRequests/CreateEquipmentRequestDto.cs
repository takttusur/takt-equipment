using System.ComponentModel.DataAnnotations;

namespace TaktTusur.Equipment.Api.Contracts.EquipmentRequests;

public class CreateEquipmentRequestDto : IValidatableObject
{
    [Required]
    [MinLength(1)]
    public string RequestText { get; set; } = string.Empty;

    public DateTime DateOfIssue { get; set; }
    public DateTime? DateOfReturn { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DateOfIssue == default)
        {
            yield return new ValidationResult(
                "Date of issue is required.",
                [nameof(DateOfIssue)]);
        }

        if (DateOfReturn.HasValue && DateOfReturn.Value < DateOfIssue)
        {
            yield return new ValidationResult(
                "Date of return cannot be earlier than date of issue.",
                [nameof(DateOfReturn)]);
        }
    }
}
