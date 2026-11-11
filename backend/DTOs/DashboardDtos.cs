using Api.Models;

namespace Api.DTOs;

public record StatusCountDto(TicketStatus Status, int Count);

public record PriorityCountDto(TicketPriority Priority, int Count);

/// <param name="Open">New + InProgress.</param>
public record TechnicianLoadDto(UserRefDto Technician, int Open, int Total);

/// <summary>A device whose service is coming up (or overdue, when DaysLeft is negative).</summary>
public record UpcomingServiceDto(
    int AssetId,
    string Name,
    string InventoryNumber,
    string? Barcode,
    DateOnly NextInspectionDate,
    int DaysLeft,
    UserRefDto? Owner);

/// <summary>Start page of the admin: nearest services and the open tickets that need attention first.</summary>
public record AdminOverviewDto(
    IReadOnlyList<UpcomingServiceDto> UpcomingServices,
    IReadOnlyList<TicketListItemDto> OpenTickets);

public record DashboardStatsDto(
    int Total,
    int Open,
    int UnassignedOpen,
    IReadOnlyList<StatusCountDto> ByStatus,
    IReadOnlyList<PriorityCountDto> ByPriority,
    IReadOnlyList<TechnicianLoadDto> ByTechnician);
