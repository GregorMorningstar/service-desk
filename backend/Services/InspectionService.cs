using Api.Auth;
using Api.Common;
using Api.DTOs;
using Api.Models;
using Api.Repositories;

namespace Api.Services;

public interface IInspectionService
{
    Task<IReadOnlyList<InspectionDto>> GetAllAsync(int? assetId);
    Task<InspectionDetailsDto> GetAsync(int id);
    Task<IReadOnlyList<PendingInspectionRecordDto>> GetPendingAsync();
    Task<InspectionRecordDto> AddRecordAsync(int inspectionId, SaveInspectionRecordDto dto);
    Task<InspectionRecordDto> UpdateRecordAsync(int recordId, SaveInspectionRecordDto dto);
    Task DeleteRecordAsync(int recordId);
    Task<InspectionRecordDto> ReviewAsync(int recordId, ReviewInspectionRecordDto dto);
}

public class InspectionService(
    IInspectionRepository inspections,
    ICurrentUser currentUser,
    TimeProvider time) : IInspectionService
{
    private bool IsStaff => currentUser.IsAdmin || currentUser.IsTechnician;
    private DateTime Now => time.GetUtcNow().UtcDateTime;

    public async Task<IReadOnlyList<InspectionDto>> GetAllAsync(int? assetId)
    {
        // Regular users only see the cards of their own devices.
        var list = await inspections.SearchAsync(assetId, IsStaff ? null : currentUser.Id);
        return list.Select(ToDto).ToList();
    }

    public async Task<InspectionDetailsDto> GetAsync(int id)
    {
        var card = await inspections.GetByIdAsync(id);

        // Someone else's card looks the same as a missing one.
        if (card is null || (!IsStaff && card.OwnerId != currentUser.Id))
            throw new NotFoundException("Nie znaleziono karty przeglądów.");

        var records = card.Records
            .OrderByDescending(r => r.PerformedAt).ThenByDescending(r => r.Id)
            .Select(ToDto)
            .ToList();

        return new InspectionDetailsDto(
            card.Id, AssetRef(card.Asset!), AddressDto.From(card.Address),
            card.Owner is null ? null : UserRefDto.From(card.Owner),
            card.Asset!.InspectionIntervalMonths, card.Asset.NextInspectionDate, records);
    }

    public async Task<IReadOnlyList<PendingInspectionRecordDto>> GetPendingAsync()
    {
        var list = await inspections.GetPendingAsync();
        return list
            .Select(r => new PendingInspectionRecordDto(r.InspectionId, AssetRef(r.Inspection!.Asset!), ToDto(r)))
            .ToList();
    }

    public async Task<InspectionRecordDto> AddRecordAsync(int inspectionId, SaveInspectionRecordDto dto)
    {
        var card = await inspections.GetByIdAsync(inspectionId)
            ?? throw new NotFoundException("Nie znaleziono karty przeglądów.");
        Validate(dto);

        // An admin's entry counts at once; a technician's waits for approval.
        var approved = currentUser.IsAdmin;
        var record = new InspectionRecord
        {
            InspectionId = card.Id,
            Kind = dto.Kind!.Value,
            Status = approved ? ReviewStatus.Approved : ReviewStatus.Pending,
            CreatedById = currentUser.Id,
            CreatedAt = Now,
            ReviewedById = approved ? currentUser.Id : null,
            ReviewedAt = approved ? Now : null,
        };
        Apply(record, dto);

        inspections.AddRecord(record);
        await inspections.SaveChangesAsync();

        if (approved)
            await RecalculateNextAsync(card.Id);

        return ToDto((await inspections.GetRecordAsync(record.Id))!);
    }

    public async Task<InspectionRecordDto> UpdateRecordAsync(int recordId, SaveInspectionRecordDto dto)
    {
        var record = await FindRecordAsync(recordId);
        Validate(dto);

        record.Kind = dto.Kind!.Value;
        Apply(record, dto);
        await inspections.SaveChangesAsync();

        await RecalculateNextAsync(record.InspectionId);
        return ToDto(record);
    }

    public async Task DeleteRecordAsync(int recordId)
    {
        var record = await FindRecordAsync(recordId);
        inspections.RemoveRecord(record);
        await inspections.SaveChangesAsync();

        await RecalculateNextAsync(record.InspectionId);
    }

    public async Task<InspectionRecordDto> ReviewAsync(int recordId, ReviewInspectionRecordDto dto)
    {
        var record = await FindRecordAsync(recordId);
        if (record.Status != ReviewStatus.Pending)
            throw new ConflictException("Ten wpis został już rozpatrzony.");

        record.Status = dto.Approve!.Value ? ReviewStatus.Approved : ReviewStatus.Rejected;
        record.ReviewedById = currentUser.Id;
        record.ReviewedAt = Now;
        await inspections.SaveChangesAsync();

        await RecalculateNextAsync(record.InspectionId);
        return ToDto((await inspections.GetRecordAsync(recordId))!);
    }

    private async Task<InspectionRecord> FindRecordAsync(int recordId) =>
        await inspections.GetRecordAsync(recordId) ?? throw new NotFoundException("Nie znaleziono wpisu.");

    /// <summary>Sets the device's planned date from its latest approved inspection.</summary>
    private async Task RecalculateNextAsync(int inspectionId)
    {
        var records = await inspections.GetRecordsAsync(inspectionId);
        var last = records
            .Where(r => r.Kind == InspectionKind.Inspection && r.Status == ReviewStatus.Approved)
            .OrderByDescending(r => r.PerformedAt).ThenByDescending(r => r.Id)
            .FirstOrDefault();
        if (last is null)
            return;

        var card = await inspections.GetTrackedWithAssetAsync(inspectionId);
        card.Asset!.NextInspectionDate = InspectionRules.NextAfter(last, card.Asset.InspectionIntervalMonths);
        await inspections.SaveChangesAsync();
    }

    private void Validate(SaveInspectionRecordDto dto)
    {
        var today = DateOnly.FromDateTime(Now);
        if (dto.PerformedAt > today)
            throw ValidationException.For("performedAt", "Data wykonania nie może być z przyszłości.");
        if (dto.NextInspectionDate is { } next && next < dto.PerformedAt)
            throw ValidationException.For("nextInspectionDate", "Następny przegląd nie może być przed datą wykonania.");
    }

    private static void Apply(InspectionRecord record, SaveInspectionRecordDto dto)
    {
        record.PerformedAt = dto.PerformedAt!.Value;
        record.Cost = dto.Cost;
        // A planned next date only makes sense for inspections, not for failure handling.
        record.NextInspectionDate = record.Kind == InspectionKind.Inspection ? dto.NextInspectionDate : null;
        record.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
    }

    private static AssetRefDto AssetRef(Asset a) => new(a.Id, a.Name, a.InventoryNumber);

    private static InspectionDto ToDto(Inspection i) => new(
        i.Id, AssetRef(i.Asset!), AddressDto.From(i.Address),
        i.Owner is null ? null : UserRefDto.From(i.Owner),
        i.Asset!.InspectionIntervalMonths, i.Asset.NextInspectionDate,
        i.Records.Count, i.Records.Count(r => r.Status == ReviewStatus.Pending));

    private static InspectionRecordDto ToDto(InspectionRecord r) => new(
        r.Id, r.Kind, r.Status, r.PerformedAt, r.Cost, r.NextInspectionDate, r.Description,
        UserRefDto.From(r.CreatedBy!), r.CreatedAt,
        r.ReviewedBy is null ? null : UserRefDto.From(r.ReviewedBy), r.ReviewedAt);
}
