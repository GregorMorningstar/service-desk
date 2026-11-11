using Api.Data;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Repositories;

public record CategoryWithCount(AssetCategory Category, int AssetCount);

public record ActiveTicketCounts(int Low, int Medium, int High, int Critical);

public interface IAssetCategoryRepository
{
    /// <param name="ownerId">Count only this user's devices (null = all devices).</param>
    Task<List<CategoryWithCount>> GetAllAsync(string? search = null, string? ownerId = null);
    Task<AssetCategory?> GetByIdAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> NameExistsAsync(string name, int? exceptId = null);
    Task<int> CountAssetsAsync(int categoryId, string? ownerId = null);

    /// <summary>Open tickets of devices, grouped by device category and priority.</summary>
    Task<Dictionary<int, ActiveTicketCounts>> GetActiveTicketsAsync(int? categoryId = null);
    void Add(AssetCategory category);
    void Remove(AssetCategory category);
    Task SaveChangesAsync();
}

public class AssetCategoryRepository(AppDbContext db) : IAssetCategoryRepository
{
    public Task<List<CategoryWithCount>> GetAllAsync(string? search = null, string? ownerId = null)
    {
        var query = db.AssetCategories.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(c => c.Name.Contains(term));
        }

        return query
            .OrderBy(c => c.Name)
            .Select(c => new CategoryWithCount(c, c.Assets.Count(a => ownerId == null || a.OwnerId == ownerId)))
            .ToListAsync();
    }

    public Task<AssetCategory?> GetByIdAsync(int id) => db.AssetCategories.FirstOrDefaultAsync(c => c.Id == id);

    public Task<bool> ExistsAsync(int id) => db.AssetCategories.AnyAsync(c => c.Id == id);

    public Task<bool> NameExistsAsync(string name, int? exceptId = null) =>
        db.AssetCategories.AnyAsync(c => c.Name == name && c.Id != exceptId);

    public Task<int> CountAssetsAsync(int categoryId, string? ownerId = null) =>
        db.Assets.CountAsync(a => a.CategoryId == categoryId && (ownerId == null || a.OwnerId == ownerId));

    public async Task<Dictionary<int, ActiveTicketCounts>> GetActiveTicketsAsync(int? categoryId = null)
    {
        var query = db.Tickets.AsNoTracking()
            .Where(t => t.AssetId != null && (t.Status == TicketStatus.New || t.Status == TicketStatus.InProgress));

        if (categoryId is { } id) query = query.Where(t => t.Asset!.CategoryId == id);

        var rows = await query
            .GroupBy(t => new { t.Asset!.CategoryId, t.Priority })
            .Select(g => new PriorityRow(g.Key.CategoryId, g.Key.Priority, g.Count()))
            .ToListAsync();

        return rows
            .GroupBy(r => r.CategoryId)
            .ToDictionary(
                g => g.Key,
                g => new ActiveTicketCounts(
                    CountOf(g, TicketPriority.Low),
                    CountOf(g, TicketPriority.Medium),
                    CountOf(g, TicketPriority.High),
                    CountOf(g, TicketPriority.Critical)));
    }

    private record PriorityRow(int CategoryId, TicketPriority Priority, int Count);

    private static int CountOf(IEnumerable<PriorityRow> rows, TicketPriority priority) =>
        rows.Where(r => r.Priority == priority).Sum(r => r.Count);

    public void Add(AssetCategory category) => db.AssetCategories.Add(category);

    public void Remove(AssetCategory category) => db.AssetCategories.Remove(category);

    public Task SaveChangesAsync() => db.SaveChangesAsync();
}
