using Api.Data;

namespace Api.Models;

/// <summary>One inspection or failure handling on an inspection card.</summary>
public class InspectionRecord
{
    public int Id { get; set; }

    public int InspectionId { get; set; }
    public Inspection? Inspection { get; set; }

    public InspectionKind Kind { get; set; }
    public ReviewStatus Status { get; set; }

    /// <summary>Day the inspection / repair was done.</summary>
    public DateOnly PerformedAt { get; set; }

    public decimal Cost { get; set; }

    /// <summary>Planned next inspection. Empty = performed date + the device's interval.</summary>
    public DateOnly? NextInspectionDate { get; set; }

    public string? Description { get; set; }

    public string CreatedById { get; set; } = string.Empty;
    public AppUser? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }

    public string? ReviewedById { get; set; }
    public AppUser? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
}
