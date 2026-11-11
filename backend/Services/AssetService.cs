using Api.Auth;
using Api.Common;
using Api.Data;
using Api.DTOs;
using Api.Models;
using Api.Repositories;
using Microsoft.AspNetCore.Identity;

namespace Api.Services;

public interface IAssetService
{
    Task<IReadOnlyList<AssetDto>> GetAllAsync(AssetQueryDto query);
    Task<AssetDto> GetAsync(int id);
    Task<AssetDto> CreateAsync(SaveAssetDto dto);
    Task<AssetDto> RequestAsync(RequestAssetDto dto);
    Task<AssetAvailabilityDto> CheckAvailabilityAsync(AssetAvailabilityQueryDto query);
    Task<IReadOnlyList<KnownAddressDto>> GetKnownAddressesAsync(KnownAddressQueryDto query);
    Task<AssetDto> ReviewAsync(int id, bool approve);
    Task<AssetDto> SetActiveAsync(int id, bool isActive);
    Task<AssetDto> UpdateAsync(int id, SaveAssetDto dto);
    Task DeleteAsync(int id);
    Task<IReadOnlyList<TicketListItemDto>> GetTicketsAsync(int id);
    Task<IReadOnlyList<AssetHistoryDto>> GetHistoryAsync(int id);
}

public class AssetService(
    IAssetRepository assets,
    IAssetCategoryRepository categories,
    IInspectionRepository inspections,
    ITicketRepository tickets,
    IPhotoStorage photoStorage,
    UserManager<AppUser> userManager,
    ICurrentUser currentUser,
    TimeProvider time) : IAssetService
{
    private bool IsStaff => currentUser.IsAdmin || currentUser.IsTechnician;
    private DateTime Now => time.GetUtcNow().UtcDateTime;
    private DateOnly Today => DateOnly.FromDateTime(Now);

    public async Task<IReadOnlyList<AssetDto>> GetAllAsync(AssetQueryDto q)
    {
        // Regular users only ever see their own devices, whatever filters they send.
        var filter = IsStaff
            ? new AssetFilter(q.CategoryId, q.Search, q.UserId, q.Owner, q.Address, q.IsActive, q.Status)
            : new AssetFilter(q.CategoryId, q.Search, currentUser.Id);
        var list = await assets.SearchAsync(filter);
        return list.Select(a => a.ToDto()).ToList();
    }

    public async Task<AssetDto> GetAsync(int id) => (await GetVisibleAsync(id)).ToDto();

    public async Task<AssetDto> CreateAsync(SaveAssetDto dto)
    {
        var owner = await ValidateAsync(dto, exceptId: null);

        var asset = new Asset();
        Apply(asset, dto);
        asset.IsActive = dto.IsActive ?? true;
        asset.InspectionIntervalMonths = dto.InspectionIntervalMonths ?? Asset.DefaultInspectionIntervalMonths;
        asset.NextInspectionDate = dto.NextInspectionDate ?? Today.AddMonths(asset.InspectionIntervalMonths);
        InspectionRules.SyncCard(asset);
        asset.History.Add(Entry("Created", null, asset.InventoryNumber));
        if (owner is not null)
            asset.History.Add(Entry("Owner", null, owner.Email));

        assets.Add(asset);
        await assets.SaveChangesAsync();

        return (await assets.GetByIdAsync(asset.Id))!.ToDto();
    }

    /// <summary>The user's own address plus the addresses of the devices they already have, without repeats.</summary>
    public async Task<IReadOnlyList<KnownAddressDto>> GetKnownAddressesAsync(KnownAddressQueryDto q)
    {
        var userId = IsStaff && !string.IsNullOrWhiteSpace(q.UserId) ? q.UserId : currentUser.Id;
        var result = new List<KnownAddressDto>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        void Add(string label, Address? address)
        {
            if (address is not null && seen.Add(address.ToString()))
                result.Add(new KnownAddressDto($"{label}: {address}", AddressDto.From(address)!));
        }

        var user = await userManager.FindByIdAsync(userId);
        Add("Adres użytkownika", user?.Address);

        foreach (var asset in await assets.SearchAsync(new AssetFilter(OwnerId: userId)))
            Add(asset.InventoryNumber, asset.Address);

        return result;
    }

    public async Task<AssetAvailabilityDto> CheckAvailabilityAsync(AssetAvailabilityQueryDto q)
    {
        var serial = q.SerialNumber?.Trim();
        var inventory = q.InventoryNumber?.Trim();
        return new AssetAvailabilityDto(
            !string.IsNullOrEmpty(serial) && await assets.SerialNumberExistsAsync(serial, q.ExceptId),
            !string.IsNullOrEmpty(inventory) && await assets.InventoryNumberExistsAsync(inventory, q.ExceptId));
    }

    public async Task<AssetDto> RequestAsync(RequestAssetDto dto)
    {
        var save = new SaveAssetDto
        {
            Name = dto.Name,
            CategoryId = dto.CategoryId,
            SerialNumber = dto.SerialNumber,
            InventoryNumber = dto.InventoryNumber,
            Description = dto.Description,
            Location = dto.Location,
            Address = dto.Address,
            OwnerId = currentUser.Id,
        };
        await ValidateAsync(save, exceptId: null);

        var asset = new Asset();
        Apply(asset, save);
        asset.InspectionIntervalMonths = Asset.DefaultInspectionIntervalMonths;
        asset.History.Add(Entry("Created", null, asset.InventoryNumber));

        // Admin's own devices need no approval; everyone else waits for one.
        if (currentUser.IsAdmin)
        {
            asset.NextInspectionDate = Today.AddMonths(asset.InspectionIntervalMonths);
            InspectionRules.SyncCard(asset);
        }
        else
        {
            asset.ApprovalStatus = ReviewStatus.Pending;
            asset.IsActive = false;
            asset.History.Add(Entry("Approval", null, nameof(ReviewStatus.Pending)));
        }

        assets.Add(asset);
        await assets.SaveChangesAsync();
        return (await assets.GetByIdAsync(asset.Id))!.ToDto();
    }

    public async Task<AssetDto> ReviewAsync(int id, bool approve)
    {
        var asset = await assets.GetByIdAsync(id) ?? throw new NotFoundException("Nie znaleziono urządzenia.");
        if (asset.ApprovalStatus != ReviewStatus.Pending)
            throw new ConflictException("To urządzenie zostało już rozpatrzone.");

        var status = approve ? ReviewStatus.Approved : ReviewStatus.Rejected;
        Track(asset, "Approval", asset.ApprovalStatus.ToString(), status.ToString());
        asset.ApprovalStatus = status;

        if (approve)
        {
            Track(asset, "Active", asset.IsActive.ToString(), bool.TrueString);
            asset.IsActive = true;
            asset.NextInspectionDate ??= Today.AddMonths(asset.InspectionIntervalMonths);
            InspectionRules.SyncCard(asset);
        }

        await assets.SaveChangesAsync();
        return asset.ToDto();
    }

    public async Task<AssetDto> SetActiveAsync(int id, bool isActive)
    {
        var asset = await assets.GetByIdAsync(id) ?? throw new NotFoundException("Nie znaleziono urządzenia.");
        if (asset.ApprovalStatus != ReviewStatus.Approved)
            throw new ConflictException("Najpierw zatwierdź urządzenie.");

        Track(asset, "Active", asset.IsActive.ToString(), isActive.ToString());
        asset.IsActive = isActive;
        await assets.SaveChangesAsync();
        return asset.ToDto();
    }

    public async Task<AssetDto> UpdateAsync(int id, SaveAssetDto dto)
    {
        var asset = await assets.GetByIdAsync(id) ?? throw new NotFoundException("Nie znaleziono urządzenia.");
        var newOwner = await ValidateAsync(dto, exceptId: id);

        if (asset.CategoryId != dto.CategoryId)
        {
            var newCategory = await categories.GetByIdAsync(dto.CategoryId!.Value);
            Track(asset, "Category", asset.Category?.Name, newCategory?.Name);
        }

        Track(asset, "Name", asset.Name, dto.Name.Trim());
        Track(asset, "SerialNumber", asset.SerialNumber, dto.SerialNumber.Trim());
        Track(asset, "InventoryNumber", asset.InventoryNumber, dto.InventoryNumber.Trim());
        Track(asset, "Location", asset.Location, Clean(dto.Location));
        Track(asset, "Description", asset.Description, Clean(dto.Description));
        if (dto.IsActive is { } active && asset.ApprovalStatus == ReviewStatus.Approved)
        {
            Track(asset, "Active", asset.IsActive.ToString(), active.ToString());
            asset.IsActive = active;
        }
        Track(asset, "Address", asset.Address?.ToString(), dto.Address?.ApplyTo(null).ToString());
        if (asset.OwnerId != dto.OwnerId)
            Track(asset, "Owner", asset.Owner?.Email, newOwner?.Email);

        await ApplyInspectionScheduleAsync(asset, dto);

        Apply(asset, dto);
        InspectionRules.SyncCard(asset);
        await assets.SaveChangesAsync();

        return (await assets.GetByIdAsync(id))!.ToDto();
    }

    public async Task DeleteAsync(int id)
    {
        var asset = await assets.GetByIdAsync(id) ?? throw new NotFoundException("Nie znaleziono urządzenia.");
        assets.Remove(asset); // tickets stay, their AssetId becomes null
        await assets.SaveChangesAsync();
        photoStorage.DeleteAll(id);
    }

    public async Task<IReadOnlyList<TicketListItemDto>> GetTicketsAsync(int id)
    {
        await GetVisibleAsync(id);
        var list = await tickets.GetByAssetAsync(id);
        return list.Select(t => t.ToListItem()).ToList();
    }

    public async Task<IReadOnlyList<AssetHistoryDto>> GetHistoryAsync(int id)
    {
        await GetVisibleAsync(id);
        var history = await assets.GetHistoryAsync(id);
        return history.Select(h => h.ToDto()).ToList();
    }

    private async Task<Asset> GetVisibleAsync(int id)
    {
        var asset = await assets.GetByIdAsync(id);

        // Someone else's device looks the same as a missing one.
        if (asset is null || (!IsStaff && asset.OwnerId != currentUser.Id))
            throw new NotFoundException("Nie znaleziono urządzenia.");

        return asset;
    }

    /// <returns>The new owner, if one was given.</returns>
    private async Task<AppUser?> ValidateAsync(SaveAssetDto dto, int? exceptId)
    {
        if (!await categories.ExistsAsync(dto.CategoryId!.Value))
            throw ValidationException.For("categoryId", "Wybrana kategoria nie istnieje.");
        if (await assets.SerialNumberExistsAsync(dto.SerialNumber.Trim(), exceptId))
            throw new ConflictException("Urządzenie o tym numerze seryjnym już istnieje.");
        if (await assets.InventoryNumberExistsAsync(dto.InventoryNumber.Trim(), exceptId))
            throw new ConflictException("Urządzenie o tym numerze inwentarzowym już istnieje.");

        if (dto.OwnerId is null)
            return null;

        return await userManager.FindByIdAsync(dto.OwnerId)
            ?? throw ValidationException.For("ownerId", "Wybrany użytkownik nie istnieje.");
    }

    /// <summary>
    /// New interval without an explicit date: the next inspection is counted from the last approved
    /// one (or from today when there has been none). An explicit date always wins.
    /// </summary>
    private async Task ApplyInspectionScheduleAsync(Asset asset, SaveAssetDto dto)
    {
        var oldInterval = asset.InspectionIntervalMonths;
        var oldNext = asset.NextInspectionDate;

        var interval = dto.InspectionIntervalMonths ?? oldInterval;
        var next = dto.NextInspectionDate ?? oldNext;
        if (dto.NextInspectionDate is null && interval != oldInterval)
        {
            var last = await inspections.GetLastApprovedInspectionDateAsync(asset.Id) ?? Today;
            next = last.AddMonths(interval);
        }

        Track(asset, "InspectionInterval", oldInterval.ToString(), interval.ToString());
        Track(asset, "NextInspection", oldNext?.ToString("yyyy-MM-dd"), next?.ToString("yyyy-MM-dd"));

        asset.InspectionIntervalMonths = interval;
        asset.NextInspectionDate = next;
    }

    private static void Apply(Asset asset, SaveAssetDto dto)
    {
        asset.Name = dto.Name.Trim();
        asset.CategoryId = dto.CategoryId!.Value;
        asset.SerialNumber = dto.SerialNumber.Trim();
        asset.InventoryNumber = dto.InventoryNumber.Trim();
        asset.Location = Clean(dto.Location);
        asset.Description = Clean(dto.Description);
        asset.Address = dto.Address?.ApplyTo(asset.Address);
        asset.OwnerId = dto.OwnerId;
    }

    private void Track(Asset asset, string field, string? oldValue, string? newValue)
    {
        if (oldValue != newValue)
            asset.History.Add(Entry(field, oldValue, newValue));
    }

    private AssetHistory Entry(string field, string? oldValue, string? newValue) => new()
    {
        Field = field,
        OldValue = oldValue,
        NewValue = newValue,
        ChangedById = currentUser.Id,
        ChangedAt = Now,
    };

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
