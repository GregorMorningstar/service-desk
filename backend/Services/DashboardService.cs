using Api.Auth;
using Api.DTOs;
using Api.Models;
using Api.Repositories;

namespace Api.Services;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetStatsAsync();
    Task<AdminOverviewDto> GetOverviewAsync();
}

public class DashboardService(
    ITicketRepository tickets,
    IAssetRepository assets,
    IUserRepository users,
    ICurrentUser currentUser,
    TimeProvider time) : IDashboardService
{
    private const int OverviewSize = 10;

    public async Task<AdminOverviewDto> GetOverviewAsync()
    {
        var today = DateOnly.FromDateTime(time.GetUtcNow().UtcDateTime);

        var services = (await assets.GetUpcomingServicesAsync(OverviewSize))
            .Select(a => new UpcomingServiceDto(
                a.Id,
                a.Name,
                a.InventoryNumber,
                a.Barcode,
                a.NextInspectionDate!.Value,
                a.NextInspectionDate.Value.DayNumber - today.DayNumber,
                a.Owner is null ? null : UserRefDto.From(a.Owner)))
            .ToList();

        var open = (await tickets.GetOpenByUrgencyAsync(OverviewSize, currentUser.IsAdmin ? null : currentUser.Id)).Select(t => t.ToListItem()).ToList();
        return new AdminOverviewDto(services, open);
    }

    public async Task<DashboardStatsDto> GetStatsAsync()
    {
        var byStatus = await tickets.CountByStatusAsync();
        var byPriority = await tickets.CountByPriorityAsync();
        var load = await tickets.GetAssigneeLoadAsync();

        var people = (await users.GetByIdsAsync(load.Select(l => l.UserId).ToList()))
            .ToDictionary(u => u.Id);

        return new DashboardStatsDto(
            Total: byStatus.Values.Sum(),
            Open: byStatus.GetValueOrDefault(TicketStatus.New) + byStatus.GetValueOrDefault(TicketStatus.InProgress),
            UnassignedOpen: await tickets.CountUnassignedOpenAsync(),
            ByStatus: Enum.GetValues<TicketStatus>()
                .Select(s => new StatusCountDto(s, byStatus.GetValueOrDefault(s)))
                .ToList(),
            ByPriority: Enum.GetValues<TicketPriority>()
                .Select(p => new PriorityCountDto(p, byPriority.GetValueOrDefault(p)))
                .ToList(),
            ByTechnician: load
                .Where(l => people.ContainsKey(l.UserId))
                .Select(l => new TechnicianLoadDto(UserRefDto.From(people[l.UserId]), l.Open, l.Total))
                .OrderByDescending(t => t.Open)
                .ToList());
    }
}
