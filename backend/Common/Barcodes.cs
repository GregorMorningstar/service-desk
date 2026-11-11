using System.ComponentModel.DataAnnotations;

namespace Api.Common;

/// <summary>Four-digit prefixes of the barcodes, set in the environment (Barcode__AssetPrefix, ...).</summary>
public class BarcodeOptions
{
    public const string Section = "Barcode";

    [RegularExpression(@"^\d{4}$", ErrorMessage = "Prefix must be exactly 4 digits.")]
    public string AssetPrefix { get; set; } = "1001";

    [RegularExpression(@"^\d{4}$", ErrorMessage = "Prefix must be exactly 4 digits.")]
    public string TicketPrefix { get; set; } = "2001";

    [RegularExpression(@"^\d{4}$", ErrorMessage = "Prefix must be exactly 4 digits.")]
    public string AssetCategoryPrefix { get; set; } = "3001";
}

/// <summary>Startup check: each kind of object needs its own prefix, or barcodes could collide.</summary>
public class BarcodeOptionsValidator : Microsoft.Extensions.Options.IValidateOptions<BarcodeOptions>
{
    public Microsoft.Extensions.Options.ValidateOptionsResult Validate(string? name, BarcodeOptions o) =>
        new[] { o.AssetPrefix, o.TicketPrefix, o.AssetCategoryPrefix }.Distinct().Count() == 3
            ? Microsoft.Extensions.Options.ValidateOptionsResult.Success
            : Microsoft.Extensions.Options.ValidateOptionsResult.Fail("Barcode prefixes of assets, tickets and categories must all differ.");
}

public static class Barcodes
{
    public const int Length = 13;

    /// <summary>Prefix, then zeros, then the id: 13 digits in total (e.g. 1001000000042).</summary>
    public static string Create(string prefix, int id) =>
        prefix + id.ToString().PadLeft(Length - prefix.Length, '0');
}
