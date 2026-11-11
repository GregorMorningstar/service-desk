namespace Api.Models;

/// <summary>
/// Postal address. Stored as an owned type: UserAddresses for users, AssetAddresses for devices.
/// </summary>
public class Address
{
    public string Street { get; set; } = string.Empty;
    public string BuildingNumber { get; set; } = string.Empty;
    public string? ApartmentNumber { get; set; }
    public string PostalCode { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = "Polska";

    /// <summary>Copies all values from another address (so EF updates the row in place).</summary>
    public void CopyFrom(Address source)
    {
        Street = source.Street;
        BuildingNumber = source.BuildingNumber;
        ApartmentNumber = source.ApartmentNumber;
        PostalCode = source.PostalCode;
        City = source.City;
        Country = source.Country;
    }

    public override string ToString()
    {
        var number = string.IsNullOrEmpty(ApartmentNumber) ? BuildingNumber : $"{BuildingNumber}/{ApartmentNumber}";
        return $"{Street} {number}, {PostalCode} {City}, {Country}";
    }
}
