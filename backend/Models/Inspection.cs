using Api.Data;

namespace Api.Models;

/// <summary>
/// Inspection card of one device: who has it and where, plus the records below.
/// Owner and address are copies of the device's values, kept in sync when the device changes.
/// </summary>
public class Inspection
{
    public int Id { get; set; }

    public int AssetId { get; set; }
    public Asset? Asset { get; set; }

    public string? OwnerId { get; set; }
    public AppUser? Owner { get; set; }

    public Address? Address { get; set; }

    public List<InspectionRecord> Records { get; set; } = [];
}
