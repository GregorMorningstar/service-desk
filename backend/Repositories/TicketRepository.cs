using Api.Common;
using Api.Data;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Repositories;

/// <param name="VisibleTo">Limit to tickets authored by this user (null = no limit).</param>
/// <param name="IncludeAssigned">With VisibleTo: also include tickets assigned to that user.</param>
public record TicketSearch(
    TicketStatus? Status,
    TicketPriority? Priority,
    int? CategoryId,
    string? Search,
    string? AssigneeId,
    int Page,
    int PageSize,
    string? VisibleTo,
    bool IncludeAssigned);

public record AssigneeLoad(string UserId, int Open, int Total);

public interface ITicketRepository
{
    Task<PagedResult<Ticket>> SearchAsync(TicketSearch search);
    Task<Ticket?> GetByIdAsync(int id);
    Task<List<Ticket>> GetByAssetAsync(int assetId);
    Task<List<Ticket>> GetByAssetCategoryAsync(int assetCategoryId);
    Task CreateAsync(Ticket ticket);

    Task<List<Comment>> GetCommentsAsync(int ticketId);
    void AddComment(Comment comment);
    Task<List<TicketHistory>> GetHistoryAsync(int ticketId);

    Task<Dictionary<TicketStatus, int>> CountByStatusAsync();
    Task<Dictionary<TicketPriority, int>> CountByPriorityAsync();
    Task<int> CountUnassignedOpenAsync();
    Task<List<Ticket>> GetOpenByUrgencyAsync(int take, string? assigneeId = null);
    Task<List<AssigneeLoad>> GetAssigneeLoadAsync();

    Task SaveChangesAsync();
}

public class TicketRepository(AppDbContext db) : ITicketRepository
{
    public async Task<PagedResult<Ticket>> SearchAsync(TicketSearch s)
    {
        var query = db.Tickets.AsNoTracking()
            .Include(t => t.Author)
            .Include(t => t.Assignee)
            .Include(t => t.Category)
            .Include(t => t.Asset)
            .AsQueryable();

        if (s.VisibleTo is { } userId)
        {
            query = s.IncludeAssigned
                ? query.Where(t => t.AuthorId == userId || t.AssigneeId == userId)
                : query.Where(t => t.AuthorId == userId);
        }

        if (s.Status is { } status) query = query.Where(t => t.Status == status);
        if (s.Priority is { } priority) query = query.Where(t => t.Priority == priority);
        if (s.CategoryId is { } categoryId) query = query.Where(t => t.CategoryId == categoryId);
        if (!string.IsNullOrWhiteSpace(s.AssigneeId)) query = query.Where(t => t.AssigneeId == s.AssigneeId);

        if (!string.IsNullOrWhiteSpace(s.Search))
        {
            var term = s.Search.Trim();
            query = query.Where(t =>
                t.Number.Contains(term) || t.Title.Contains(term) || t.Description.Contains(term));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .ThenByDescending(t => t.Id)
            .Skip((s.Page - 1) * s.PageSize)
            .Take(s.PageSize)
            .ToListAsync();

        return new PagedResult<Ticket>(items, s.Page, s.PageSize, total);
    }

    public Task<Ticket?> GetByIdAsync(int id) =>
        db.Tickets
            .Include(t => t.Author)
            .Include(t => t.Assignee)
            .Include(t => t.Asset)
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == id);

    public Task<List<Ticket>> GetByAssetAsync(int assetId) =>
        db.Tickets.AsNoTracking()
            .Include(t => t.Author)
            .Include(t => t.Assignee)
            .Include(t => t.Category)
            .Where(t => t.AssetId == assetId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

    public Task<List<Ticket>> GetByAssetCategoryAsync(int assetCategoryId) =>
        db.Tickets.AsNoTracking()
            .Include(t => t.Author)
            .Include(t => t.Assignee)
            .Include(t => t.Category)
            .Where(t => t.Asset!.CategoryId == assetCategoryId)
            .OrderByDescending(t => t.CreatedAt)
            .ThenByDescending(t => t.Id)
            .ToListAsync();

    public async Task CreateAsync(Ticket ticket)
    {
        // The number is built from the generated Id, so insert first and fix it up in the same transaction.
        await using var tx = await db.Database.BeginTransactionAsync();
        ticket.Number = Ticket.TemporaryNumber();
        db.Tickets.Add(ticket);
        await db.SaveChangesAsync();

        ticket.Number = Ticket.FormatNumber(ticket.Id);
        await db.SaveChangesAsync();
        await tx.CommitAsync();
    }

    public Task<List<Comment>> GetCommentsAsync(int ticketId) =>
        db.Comments.AsNoTracking()
            .Include(c => c.Author)
            .Where(c => c.TicketId == ticketId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

    public void AddComment(Comment comment) => db.Comments.Add(comment);

    public Task<List<TicketHistory>> GetHistoryAsync(int ticketId) =>
        db.TicketHistory.AsNoTracking()
            .Include(h => h.ChangedBy)
            .Where(h => h.TicketId == ticketId)
            .OrderBy(h => h.ChangedAt)
            .ToListAsync();

    public Task<Dictionary<TicketStatus, int>> CountByStatusAsync() =>
        db.Tickets.GroupBy(t => t.Status)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count);

    public Task<Dictionary<TicketPriority, int>> CountByPriorityAsync() =>
        db.Tickets.GroupBy(t => t.Priority)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count);

    /// <summary>Open tickets, the most urgent first (priority, then the oldest).</summary>
    public Task<List<Ticket>> GetOpenByUrgencyAsync(int take, string? assigneeId = null) =>
        db.Tickets.AsNoTracking()
            .Include(t => t.Author)
            .Include(t => t.Assignee)
            .Include(t => t.Category)
            .Include(t => t.Asset)
            .Where(t => t.Status == TicketStatus.New || t.Status == TicketStatus.InProgress)
            .Where(t => assigneeId == null || t.AssigneeId == assigneeId)
            .OrderByDescending(t => t.Priority)
            .ThenBy(t => t.CreatedAt)
            .Take(take)
            .ToListAsync();

    public Task<int> CountUnassignedOpenAsync() =>
        db.Tickets.CountAsync(t =>
            t.AssigneeId == null && (t.Status == TicketStatus.New || t.Status == TicketStatus.InProgress));

    public Task<List<AssigneeLoad>> GetAssigneeLoadAsync() =>
        db.Tickets
            .Where(t => t.AssigneeId != null)
            .GroupBy(t => t.AssigneeId!)
            .Select(g => new AssigneeLoad(
                g.Key,
                g.Count(t => t.Status == TicketStatus.New || t.Status == TicketStatus.InProgress),
                g.Count()))
            .ToListAsync();

    public Task SaveChangesAsync() => db.SaveChangesAsync();
}
