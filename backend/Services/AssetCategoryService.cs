using Api.Auth;
using Api.Common;
using Api.DTOs;
using Api.Models;
using Api.Repositories;

namespace Api.Services;

public interface IAssetCategoryService
{
    Task<IReadOnlyList<AssetCategoryDto>> GetAllAsync(string? search = null);
    Task<AssetCategoryDto> GetAsync(int id);
    Task<IReadOnlyList<TicketListItemDto>> GetTicketsAsync(int id);
    Task<AssetCategoryDto> CreateAsync(SaveAssetCategoryDto dto);
    Task<AssetCategoryDto> UpdateAsync(int id, SaveAssetCategoryDto dto);
    Task DeleteAsync(int id);
}

public class AssetCategoryService(
    IAssetCategoryRepository categories,
    ITicketRepository tickets,
    ICurrentUser currentUser) : IAssetCategoryService
{
    private bool IsStaff => currentUser.IsAdmin || currentUser.IsTechnician;

    /// <summary>Regular users only count their own devices; staff count every device.</summary>
    private string? OwnerScope => IsStaff ? null : currentUser.Id;

    /// <summary>Open-ticket counts are not shown to regular users: they would reveal other people's tickets.</summary>
    private async Task<Dictionary<int, ActiveTicketCounts>> ActiveTicketsAsync(int? categoryId = null) =>
        IsStaff ? await categories.GetActiveTicketsAsync(categoryId) : [];

    public async Task<IReadOnlyList<AssetCategoryDto>> GetAllAsync(string? search = null)
    {
        var list = await categories.GetAllAsync(search, OwnerScope);
        var active = await ActiveTicketsAsync();
        return list.Select(c => ToDto(c.Category, c.AssetCount, active.GetValueOrDefault(c.Category.Id))).ToList();
    }

    public async Task<AssetCategoryDto> GetAsync(int id)
    {
        var category = await FindAsync(id);
        var active = await ActiveTicketsAsync(id);
        return ToDto(category, await categories.CountAssetsAsync(id, OwnerScope), active.GetValueOrDefault(id));
    }

    public async Task<IReadOnlyList<TicketListItemDto>> GetTicketsAsync(int id)
    {
        await FindAsync(id);
        var list = await tickets.GetByAssetCategoryAsync(id);
        return list.Select(t => t.ToListItem()).ToList();
    }

    public async Task<AssetCategoryDto> CreateAsync(SaveAssetCategoryDto dto)
    {
        var name = dto.Name.Trim();
        if (await categories.NameExistsAsync(name))
            throw new ConflictException("Kategoria o tej nazwie już istnieje.");

        var category = new AssetCategory { Name = name, Description = Clean(dto.Description) };
        categories.Add(category);
        await categories.SaveChangesAsync();

        return ToDto(category, 0, null);
    }

    public async Task<AssetCategoryDto> UpdateAsync(int id, SaveAssetCategoryDto dto)
    {
        var category = await FindAsync(id);
        var name = dto.Name.Trim();
        if (await categories.NameExistsAsync(name, exceptId: id))
            throw new ConflictException("Kategoria o tej nazwie już istnieje.");

        category.Name = name;
        category.Description = Clean(dto.Description);
        await categories.SaveChangesAsync();

        var active = await ActiveTicketsAsync(id);
        return ToDto(category, await categories.CountAssetsAsync(id), active.GetValueOrDefault(id));
    }

    public async Task DeleteAsync(int id)
    {
        var category = await FindAsync(id);
        var inUse = await categories.CountAssetsAsync(id);
        if (inUse > 0)
            throw new ConflictException($"Kategoria jest używana przez {inUse} urządzeń. Najpierw zmień im kategorię.");

        categories.Remove(category);
        await categories.SaveChangesAsync();
    }

    private async Task<AssetCategory> FindAsync(int id) =>
        await categories.GetByIdAsync(id) ?? throw new NotFoundException("Nie znaleziono kategorii.");

    private static AssetCategoryDto ToDto(AssetCategory c, int assetCount, ActiveTicketCounts? active) =>
        new(c.Id, c.Name, c.Description, assetCount,
            active is null ? ActiveTicketsDto.None : new ActiveTicketsDto(active.Low, active.Medium, active.High, active.Critical),
            c.Barcode);

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
