using System.ComponentModel.DataAnnotations;
using Api.Models;

namespace Api.DTOs;

public record AddressDto
{
    [Required, StringLength(100)]
    public string Street { get; init; } = string.Empty;

    [Required, StringLength(20)]
    public string BuildingNumber { get; init; } = string.Empty;

    [StringLength(20)]
    public string? ApartmentNumber { get; init; }

    /// <summary>E.g. 00-950.</summary>
    [Required, StringLength(12)]
    public string PostalCode { get; init; } = string.Empty;

    [Required, StringLength(100)]
    public string City { get; init; } = string.Empty;

    [Required, StringLength(100)]
    public string Country { get; init; } = "Polska";

    public static AddressDto? From(Address? a) => a is null ? null : new AddressDto
    {
        Street = a.Street,
        BuildingNumber = a.BuildingNumber,
        ApartmentNumber = a.ApartmentNumber,
        PostalCode = a.PostalCode,
        City = a.City,
        Country = a.Country,
    };

    /// <summary>Copies values into an existing address (or a new one) so EF updates the row in place.</summary>
    public Address ApplyTo(Address? target)
    {
        target ??= new Address();
        target.Street = Street.Trim();
        target.BuildingNumber = BuildingNumber.Trim();
        target.ApartmentNumber = string.IsNullOrWhiteSpace(ApartmentNumber) ? null : ApartmentNumber.Trim();
        target.PostalCode = PostalCode.Trim();
        target.City = City.Trim();
        target.Country = Country.Trim();
        return target;
    }
}
