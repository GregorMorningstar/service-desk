using System.ComponentModel.DataAnnotations;
using Api.Models;

namespace Api.DTOs;

public record AssetCategoryRefDto(int Id, string Name);

public record AssetDto(
    int Id,
    string Name,
    AssetCategoryRefDto Category,
    string SerialNumber,
    string InventoryNumber,
    string? Location,
    AddressDto? Address,
    UserRefDto? Owner,
    int InspectionIntervalMonths,
    DateOnly? NextInspectionDate,
    string? Description,
    bool IsActive,
    ReviewStatus ApprovalStatus,
    string? Barcode,
    int PhotoCount);

public record AssetQueryDto
{
    public int? CategoryId { get; init; }

    /// <summary>Searches name, serial and inventory number.</summary>
    [StringLength(100)]
    public string? Search { get; init; }

    /// <summary>Owner filter. Only used by technicians and admins; regular users always get their own devices.</summary>
    public string? UserId { get; init; }

    /// <summary>Staff only: owner's name or e-mail.</summary>
    [StringLength(100)]
    public string? Owner { get; init; }

    /// <summary>Staff only: street, city or postal code.</summary>
    [StringLength(100)]
    public string? Address { get; init; }

    public bool? IsActive { get; init; }
    public ReviewStatus? Status { get; init; }
}

/// <summary>A user proposes a new device; it is added to their account once an admin approves it.</summary>
public record RequestAssetDto
{
    [Required, StringLength(100)]
    public string Name { get; init; } = string.Empty;

    [Required]
    public int? CategoryId { get; init; }

    [Required, StringLength(50)]
    public string SerialNumber { get; init; } = string.Empty;

    [Required, StringLength(30)]
    public string InventoryNumber { get; init; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; init; }

    [StringLength(100)]
    public string? Location { get; init; }

    public AddressDto? Address { get; init; }
}

public record SetActiveDto(bool IsActive);

public record KnownAddressDto(string Label, AddressDto Address);

public record KnownAddressQueryDto
{
    /// <summary>Staff only; everyone else always gets their own addresses.</summary>
    public string? UserId { get; init; }
}

public record AssetAvailabilityQueryDto
{
    [StringLength(50)]
    public string? SerialNumber { get; init; }

    [StringLength(30)]
    public string? InventoryNumber { get; init; }

    /// <summary>The device being edited, so it does not clash with itself.</summary>
    public int? ExceptId { get; init; }
}

/// <summary>Whether the numbers are already used by another device.</summary>
public record AssetAvailabilityDto(bool SerialTaken, bool InventoryTaken);

public record AssetPhotoDto(int Id, string OriginalName, string ContentType, long Size, DateTime UploadedAt);

public record SaveAssetDto
{
    [Required, StringLength(100)]
    public string Name { get; init; } = string.Empty;

    [Required]
    public int? CategoryId { get; init; }

    [Required, StringLength(50)]
    public string SerialNumber { get; init; } = string.Empty;

    /// <summary>E.g. LAP-014.</summary>
    [Required, StringLength(30)]
    public string InventoryNumber { get; init; } = string.Empty;

    [StringLength(100)]
    public string? Location { get; init; }

    /// <summary>Where the device physically is. Null removes the address.</summary>
    public AddressDto? Address { get; init; }

    public string? OwnerId { get; init; }

    [StringLength(500)]
    public string? Description { get; init; }

    /// <summary>Empty = active.</summary>
    public bool? IsActive { get; init; }

    /// <summary>How often the device is inspected, in months. Empty = keep the current value (a year for new devices).</summary>
    [Range(1, 120)]
    public int? InspectionIntervalMonths { get; init; }

    /// <summary>Planned next inspection. Empty = calculated from the interval.</summary>
    public DateOnly? NextInspectionDate { get; init; }
}

public record AssetHistoryDto(
    int Id,
    string Field,
    string? OldValue,
    string? NewValue,
    UserRefDto ChangedBy,
    DateTime ChangedAt);

/// <summary>Open (New / InProgress) tickets of the devices in a category, per priority. Staff only, zeros for users.</summary>
public record ActiveTicketsDto(int Low, int Medium, int High, int Critical)
{
    public static ActiveTicketsDto None { get; } = new(0, 0, 0, 0);
}

public record AssetCategoryDto(int Id, string Name, string? Description, int AssetCount, ActiveTicketsDto ActiveTickets, string? Barcode);

public record SaveAssetCategoryDto
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    [StringLength(255)]
    public string? Description { get; init; }
}
