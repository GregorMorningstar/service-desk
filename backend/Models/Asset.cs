using Api.Data;

namespace Api.Models;

public class Asset : IHasBarcode
{
    public int Id { get; set; }

    /// <summary>EAN-13, filled after the first save.</summary>
    public string? Barcode { get; set; }
    public string Name { get; set; } = string.Empty;

    public int CategoryId { get; set; }
    public AssetCategory? Category { get; set; }

    public string SerialNumber { get; set; } = string.Empty;
    public string InventoryNumber { get; set; } = string.Empty;

    /// <summary>Room / desk, e.g. "Pokój 2.14". The street address is in Address.</summary>
    public string? Location { get; set; }

    public Address? Address { get; set; }

    public string? Description { get; set; }

    /// <summary>Inactive devices (e.g. withdrawn) stay in the base but are switched off by an admin.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Devices added by users wait for an admin; admin-created and older devices are approved.</summary>
    public ReviewStatus ApprovalStatus { get; set; } = ReviewStatus.Approved;

    public string? OwnerId { get; set; }
    public AppUser? Owner { get; set; }

    /// <summary>How often the device is inspected. Set by an admin, a year by default.</summary>
    public int InspectionIntervalMonths { get; set; } = DefaultInspectionIntervalMonths;

    /// <summary>Planned date of the next inspection.</summary>
    public DateOnly? NextInspectionDate { get; set; }

    /// <summary>Inspection card of this device (owner, address, inspections and failure handling).</summary>
    public Inspection? Inspection { get; set; }

    public const int DefaultInspectionIntervalMonths = 12;

    public List<Ticket> Tickets { get; set; } = [];
    public List<AssetHistory> History { get; set; } = [];
    public List<AssetPhoto> Photos { get; set; } = [];
}
