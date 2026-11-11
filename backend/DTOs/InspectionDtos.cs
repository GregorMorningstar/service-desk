using System.ComponentModel.DataAnnotations;
using Api.Models;

namespace Api.DTOs;

public record InspectionRecordDto(
    int Id,
    InspectionKind Kind,
    ReviewStatus Status,
    DateOnly PerformedAt,
    decimal Cost,
    DateOnly? NextInspectionDate,
    string? Description,
    UserRefDto CreatedBy,
    DateTime CreatedAt,
    UserRefDto? ReviewedBy,
    DateTime? ReviewedAt);

/// <summary>Row of the inspection list: device, where it is, who has it and when it is due.</summary>
public record InspectionDto(
    int Id,
    AssetRefDto Asset,
    AddressDto? Address,
    UserRefDto? Owner,
    int IntervalMonths,
    DateOnly? NextInspectionDate,
    int RecordsCount,
    int PendingCount);

public record InspectionDetailsDto(
    int Id,
    AssetRefDto Asset,
    AddressDto? Address,
    UserRefDto? Owner,
    int IntervalMonths,
    DateOnly? NextInspectionDate,
    IReadOnlyList<InspectionRecordDto> Records);

/// <summary>A record waiting for the admin, with the device it belongs to.</summary>
public record PendingInspectionRecordDto(int InspectionId, AssetRefDto Asset, InspectionRecordDto Record);

public record SaveInspectionRecordDto
{
    [Required]
    public InspectionKind? Kind { get; init; }

    [Required]
    public DateOnly? PerformedAt { get; init; }

    [Range(0, 100_000_000)]
    public decimal Cost { get; init; }

    /// <summary>Empty = performed date + the device's interval (inspections only).</summary>
    public DateOnly? NextInspectionDate { get; init; }

    [StringLength(2000)]
    public string? Description { get; init; }
}

public record ReviewInspectionRecordDto
{
    [Required]
    public bool? Approve { get; init; }
}
