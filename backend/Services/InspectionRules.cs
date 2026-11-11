using Api.Models;

namespace Api.Services;

/// <summary>Rules shared by the asset and inspection services.</summary>
internal static class InspectionRules
{
    /// <summary>Makes sure the device has an inspection card and copies owner and address onto it.</summary>
    public static void SyncCard(Asset asset)
    {
        var card = asset.Inspection ??= new Inspection();
        card.OwnerId = asset.OwnerId;

        if (asset.Address is null)
        {
            card.Address = null;
        }
        else
        {
            card.Address ??= new Address();
            card.Address.CopyFrom(asset.Address);
        }
    }

    /// <summary>Planned date after the last inspection, or null when nothing is known yet.</summary>
    public static DateOnly? NextAfter(InspectionRecord last, int intervalMonths) =>
        last.NextInspectionDate ?? last.PerformedAt.AddMonths(intervalMonths);
}
