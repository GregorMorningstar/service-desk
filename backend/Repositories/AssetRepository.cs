using Api.Data;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Repositories;

public record AssetFilter(
    int? CategoryId = null,
    string? Search = null,
    string? OwnerId = null,
    string? Owner = null,
    string? Address = null,
    bool? IsActive = null,
    ReviewStatus? Status = null);

public interface IAssetRepository
{
    Task<List<Asset>> SearchAsync(AssetFilter filter);
    Task<Asset?> GetByIdAsync(int id);
    Task<List<Asset>> GetUpcomingServicesAsync(int take);
    Task<bool> SerialNumberExistsAsync(string serialNumber, int? exceptId = null);
    Task<bool> InventoryNumberExistsAsync(string inventoryNumber, int? exceptId = null);
    Task<List<AssetHistory>> GetHistoryAsync(int assetId);
    void Add(Asset asset);
    void Remove(Asset asset);
    Task SaveChangesAsync();
}

public class AssetRepository(AppDbContext db) : IAssetRepository
{
    public Task<List<Asset>> SearchAsync(AssetFilter f)
    {
        var query = db.Assets.AsNoTracking()
            .Include(a => a.Owner)
            .Include(a => a.Category)
            .Include(a => a.Photos)
            .AsQueryable();

        if (f.CategoryId is { } c) query = query.Where(a => a.CategoryId == c);
        if (!string.IsNullOrWhiteSpace(f.OwnerId)) query = query.Where(a => a.OwnerId == f.OwnerId);
        if (f.IsActive is { } active) query = query.Where(a => a.IsActive == active);
        if (f.Status is { } status) query = query.Where(a => a.ApprovalStatus == status);
        if (!string.IsNullOrWhiteSpace(f.Search))
        {
            var term = f.Search.Trim();
            query = query.Where(a =>
                a.Name.Contains(term) || a.SerialNumber.Contains(term) || a.InventoryNumber.Contains(term));
        }
        if (!string.IsNullOrWhiteSpace(f.Owner))
        {
            var term = f.Owner.Trim();
            query = query.Where(a => a.Owner != null &&
                (a.Owner.Email!.Contains(term) || a.Owner.FirstName.Contains(term) || a.Owner.LastName.Contains(term)));
        }
        if (!string.IsNullOrWhiteSpace(f.Address))
        {
            var term = f.Address.Trim();
            query = query.Where(a => a.Address != null &&
                (a.Address.Street.Contains(term) || a.Address.City.Contains(term) || a.Address.PostalCode.Contains(term)));
        }

        return query.OrderBy(a => a.InventoryNumber).ToListAsync();
    }

    /// <summary>Active, approved devices with a planned service, the nearest (or most overdue) first.</summary>
    public Task<List<Asset>> GetUpcomingServicesAsync(int take) =>
        db.Assets.AsNoTracking()
            .Include(a => a.Owner)
            .Where(a => a.IsActive && a.ApprovalStatus == ReviewStatus.Approved && a.NextInspectionDate != null)
            .OrderBy(a => a.NextInspectionDate)
            .Take(take)
            .ToListAsync();

    public Task<Asset?> GetByIdAsync(int id) =>
        db.Assets
            .Include(a => a.Owner)
            .Include(a => a.Category)
            .Include(a => a.Inspection)
            .Include(a => a.Photos)
            .FirstOrDefaultAsync(a => a.Id == id);

    public Task<bool> SerialNumberExistsAsync(string serialNumber, int? exceptId = null) =>
        db.Assets.AnyAsync(a => a.SerialNumber == serialNumber && a.Id != exceptId);

    public Task<bool> InventoryNumberExistsAsync(string inventoryNumber, int? exceptId = null) =>
        db.Assets.AnyAsync(a => a.InventoryNumber == inventoryNumber && a.Id != exceptId);

    public Task<List<AssetHistory>> GetHistoryAsync(int assetId) =>
        db.AssetHistory.AsNoTracking()
            .Include(h => h.ChangedBy)
            .Where(h => h.AssetId == assetId)
            .OrderBy(h => h.ChangedAt)
            .ThenBy(h => h.Id)
            .ToListAsync();

    public void Add(Asset asset) => db.Assets.Add(asset);

    public void Remove(Asset asset) => db.Assets.Remove(asset);

    public Task SaveChangesAsync() => db.SaveChangesAsync();
}
