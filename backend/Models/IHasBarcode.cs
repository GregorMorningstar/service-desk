namespace Api.Models;

/// <summary>An entity with an EAN-13 barcode derived from its id; AppDbContext fills it after the first save.</summary>
public interface IHasBarcode
{
    int Id { get; }
    string? Barcode { get; set; }
}
