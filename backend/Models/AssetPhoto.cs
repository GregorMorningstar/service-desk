using Api.Data;

namespace Api.Models;

/// <summary>A photo of a device. The file lives in the photo storage; only metadata is in the database.</summary>
public class AssetPhoto
{
    public int Id { get; set; }

    public int AssetId { get; set; }
    public Asset? Asset { get; set; }

    /// <summary>Generated file name (guid + extension), never taken from the client.</summary>
    public string StoredName { get; set; } = string.Empty;

    public string OriginalName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }

    public DateTime UploadedAt { get; set; }

    public string UploadedById { get; set; } = string.Empty;
    public AppUser? UploadedBy { get; set; }
}
