namespace Api.Models;

public class AssetCategory : IHasBarcode
{
    public int Id { get; set; }

    /// <summary>EAN-13, filled after the first save.</summary>
    public string? Barcode { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public List<Asset> Assets { get; set; } = [];
}
