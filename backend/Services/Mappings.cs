using Api.DTOs;
using Api.Models;

namespace Api.Services;

internal static class Mappings
{
    public static TicketListItemDto ToListItem(this Ticket t) => new(
        t.Id,
        t.Number,
        t.Title,
        t.Status,
        t.Priority,
        new TicketCategoryRefDto(t.CategoryId, t.Category?.Name ?? string.Empty),
        UserRefDto.From(t.Author!),
        t.Assignee is null ? null : UserRefDto.From(t.Assignee),
        t.CreatedAt,
        t.Barcode,
        t.Asset is null ? null : new AssetRefDto(t.Asset.Id, t.Asset.Name, t.Asset.InventoryNumber));

    public static TicketDetailsDto ToDetails(this Ticket t) => new(
        t.Id,
        t.Number,
        t.Title,
        t.Description,
        t.Status,
        t.Priority,
        new TicketCategoryRefDto(t.CategoryId, t.Category?.Name ?? string.Empty),
        UserRefDto.From(t.Author!),
        t.Assignee is null ? null : UserRefDto.From(t.Assignee),
        t.Asset is null ? null : new AssetRefDto(t.Asset.Id, t.Asset.Name, t.Asset.InventoryNumber),
        t.Resolution,
        t.CreatedAt,
        t.ResolvedAt,
        t.Barcode);

    public static CommentDto ToDto(this Comment c) =>
        new(c.Id, c.Content, UserRefDto.From(c.Author!), c.CreatedAt);

    public static TicketHistoryDto ToDto(this TicketHistory h) =>
        new(h.Id, h.Field, h.OldValue, h.NewValue, UserRefDto.From(h.ChangedBy!), h.ChangedAt);

    public static AssetDto ToDto(this Asset a) => new(
        a.Id,
        a.Name,
        new AssetCategoryRefDto(a.CategoryId, a.Category?.Name ?? string.Empty),
        a.SerialNumber,
        a.InventoryNumber,
        a.Location,
        AddressDto.From(a.Address),
        a.Owner is null ? null : UserRefDto.From(a.Owner),
        a.InspectionIntervalMonths,
        a.NextInspectionDate,
        a.Description,
        a.IsActive,
        a.ApprovalStatus,
        a.Barcode,
        a.Photos.Count);

    public static AssetHistoryDto ToDto(this AssetHistory h) =>
        new(h.Id, h.Field, h.OldValue, h.NewValue, UserRefDto.From(h.ChangedBy!), h.ChangedAt);
}
