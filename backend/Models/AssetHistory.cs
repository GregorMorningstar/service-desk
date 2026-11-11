using Api.Data;

namespace Api.Models;

public class AssetHistory
{
    public int Id { get; set; }

    public int AssetId { get; set; }
    public Asset? Asset { get; set; }

    public string ChangedById { get; set; } = string.Empty;
    public AppUser? ChangedBy { get; set; }

    public string Field { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public DateTime ChangedAt { get; set; }
}
