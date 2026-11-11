using Api.Data;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Repositories;

public interface IInspectionRepository
{
    Task<List<Inspection>> SearchAsync(int? assetId, string? ownerId);
    Task<Inspection?> GetByIdAsync(int id);
    Task<Inspection> GetTrackedWithAssetAsync(int id);
    Task<InspectionRecord?> GetRecordAsync(int recordId);
    Task<List<InspectionRecord>> GetRecordsAsync(int inspectionId);
    Task<List<InspectionRecord>> GetPendingAsync();
    Task<DateOnly?> GetLastApprovedInspectionDateAsync(int assetId);
    void AddRecord(InspectionRecord record);
    void RemoveRecord(InspectionRecord record);
    Task SaveChangesAsync();
}

public class InspectionRepository(AppDbContext db) : IInspectionRepository
{
    public async Task<List<Inspection>> SearchAsync(int? assetId, string? ownerId)
    {
        var query = db.Inspections.AsNoTracking()
            .Include(i => i.Asset)
            .Include(i => i.Owner)
            .Include(i => i.Records)
            .AsQueryable();

        if (assetId is { } a) query = query.Where(i => i.AssetId == a);
        if (!string.IsNullOrWhiteSpace(ownerId)) query = query.Where(i => i.OwnerId == ownerId);

        var list = await query.ToListAsync();

        // Soonest due first; devices without a date go last.
        return list
            .OrderBy(i => i.Asset!.NextInspectionDate is null)
            .ThenBy(i => i.Asset!.NextInspectionDate)
            .ThenBy(i => i.Asset!.InventoryNumber)
            .ToList();
    }

    public Task<Inspection?> GetByIdAsync(int id) =>
        db.Inspections.AsNoTracking()
            .Include(i => i.Asset)
            .Include(i => i.Owner)
            .Include(i => i.Records).ThenInclude(r => r.CreatedBy)
            .Include(i => i.Records).ThenInclude(r => r.ReviewedBy)
            .FirstOrDefaultAsync(i => i.Id == id);

    public Task<Inspection> GetTrackedWithAssetAsync(int id) =>
        db.Inspections.Include(i => i.Asset).FirstAsync(i => i.Id == id);

    public Task<InspectionRecord?> GetRecordAsync(int recordId) =>
        db.InspectionRecords
            .Include(r => r.Inspection).ThenInclude(i => i!.Asset)
            .Include(r => r.CreatedBy)
            .Include(r => r.ReviewedBy)
            .FirstOrDefaultAsync(r => r.Id == recordId);

    // Tracked on purpose: pending changes of the record being edited are visible to the caller.
    public Task<List<InspectionRecord>> GetRecordsAsync(int inspectionId) =>
        db.InspectionRecords.Where(r => r.InspectionId == inspectionId).ToListAsync();

    public Task<List<InspectionRecord>> GetPendingAsync() =>
        db.InspectionRecords.AsNoTracking()
            .Include(r => r.Inspection).ThenInclude(i => i!.Asset)
            .Include(r => r.CreatedBy)
            .Where(r => r.Status == ReviewStatus.Pending)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync();

    public async Task<DateOnly?> GetLastApprovedInspectionDateAsync(int assetId)
    {
        var dates = await db.InspectionRecords
            .Where(r => r.Inspection!.AssetId == assetId
                        && r.Kind == InspectionKind.Inspection
                        && r.Status == ReviewStatus.Approved)
            .Select(r => r.PerformedAt)
            .ToListAsync();
        return dates.Count == 0 ? null : dates.Max();
    }

    public void AddRecord(InspectionRecord record) => db.InspectionRecords.Add(record);

    public void RemoveRecord(InspectionRecord record) => db.InspectionRecords.Remove(record);

    public Task SaveChangesAsync() => db.SaveChangesAsync();
}
