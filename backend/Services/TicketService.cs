using Api.Auth;
using Api.Common;
using Api.Data;
using Api.DTOs;
using Api.Models;
using Api.Repositories;
using Microsoft.AspNetCore.Identity;

namespace Api.Services;

public interface ITicketService
{
    Task<PagedResult<TicketListItemDto>> GetListAsync(TicketListQueryDto query);
    Task<TicketDetailsDto> GetAsync(int id);
    Task<TicketDetailsDto> CreateAsync(SaveTicketDto dto);
    Task<TicketDetailsDto> UpdateAsync(int id, SaveTicketDto dto);
    Task ChangeStatusAsync(int id, ChangeStatusDto dto);
    Task AcceptAsync(int id, AcceptTicketDto dto);
    Task AssignAsync(int id, AssignTicketDto dto);
    Task<IReadOnlyList<CommentDto>> GetCommentsAsync(int id);
    Task<CommentDto> AddCommentAsync(int id, CreateCommentDto dto);
    Task<IReadOnlyList<TicketHistoryDto>> GetHistoryAsync(int id);
}

public class TicketService(
    ITicketRepository tickets,
    IAssetRepository assets,
    ITicketCategoryRepository categories,
    UserManager<AppUser> userManager,
    ICurrentUser currentUser,
    TimeProvider time) : ITicketService
{
    private DateTime Now => time.GetUtcNow().UtcDateTime;

    public async Task<PagedResult<TicketListItemDto>> GetListAsync(TicketListQueryDto q)
    {
        // Admin: everything. Technician: own + assigned. User: own only.
        var search = new TicketSearch(
            q.Status, q.Priority, q.CategoryId, q.Search, q.AssignedToMe ? currentUser.Id : q.AssigneeId, q.Page, q.PageSize,
            VisibleTo: currentUser.IsAdmin ? null : currentUser.Id,
            IncludeAssigned: currentUser.IsTechnician);

        var page = await tickets.SearchAsync(search);
        return page.Map(t => t.ToListItem());
    }

    public async Task<TicketDetailsDto> GetAsync(int id) => (await GetVisibleAsync(id)).ToDetails();

    public async Task<TicketDetailsDto> CreateAsync(SaveTicketDto dto)
    {
        await EnsureCategoryExistsAsync(dto.CategoryId!.Value);
        await EnsureAssetAllowedAsync(dto.AssetId);

        var ticket = new Ticket
        {
            Title = dto.Title.Trim(),
            Description = dto.Description.Trim(),
            CategoryId = dto.CategoryId!.Value,
            Priority = dto.Priority!.Value,
            AssetId = dto.AssetId,
            AuthorId = currentUser.Id,
            Status = TicketStatus.New,
            CreatedAt = Now,
        };

        await tickets.CreateAsync(ticket);
        return (await GetVisibleAsync(ticket.Id)).ToDetails();
    }

    public async Task<TicketDetailsDto> UpdateAsync(int id, SaveTicketDto dto)
    {
        var ticket = await GetVisibleAsync(id);

        if (!currentUser.IsAdmin && ticket.AuthorId != currentUser.Id)
            throw new ForbiddenException("Zgłoszenie może edytować tylko autor lub administrator.");
        if (ticket.Status == TicketStatus.Closed)
            throw new ConflictException("Zamkniętego zgłoszenia nie można edytować.");
        if (dto.AssetId != ticket.AssetId)
            await EnsureAssetAllowedAsync(dto.AssetId);

        Track(ticket, "Title", ticket.Title, dto.Title.Trim());
        if (dto.CategoryId != ticket.CategoryId)
        {
            var newCategory = await categories.GetByIdAsync(dto.CategoryId!.Value)
                ?? throw ValidationException.For("categoryId", "Wybrana kategoria nie istnieje.");
            Track(ticket, "Category", ticket.Category?.Name, newCategory.Name);
            ticket.Category = newCategory;
        }
        Track(ticket, "Priority", ticket.Priority, dto.Priority);
        Track(ticket, "AssetId", ticket.AssetId, dto.AssetId);

        ticket.Title = dto.Title.Trim();
        ticket.Description = dto.Description.Trim();
        ticket.CategoryId = dto.CategoryId!.Value;
        ticket.Priority = dto.Priority!.Value;
        ticket.AssetId = dto.AssetId;

        await tickets.SaveChangesAsync();
        return (await GetVisibleAsync(id)).ToDetails();
    }

    public async Task ChangeStatusAsync(int id, ChangeStatusDto dto)
    {
        var ticket = await GetVisibleAsync(id);
        var target = dto.Status!.Value;

        TicketStatusRules.EnsureAllowed(ticket, target, Actor, dto.Resolution, dto.Comment);

        Track(ticket, "Status", ticket.Status, target);

        if (target == TicketStatus.Resolved)
        {
            ticket.Resolution = dto.Resolution!.Trim();
            ticket.ResolvedAt = Now;
        }
        else if (ticket.Status == TicketStatus.Resolved && target == TicketStatus.InProgress)
        {
            ticket.ResolvedAt = null;
        }

        ticket.Status = target;

        if (!string.IsNullOrWhiteSpace(dto.Comment))
        {
            ticket.Comments.Add(new Comment
            {
                AuthorId = currentUser.Id,
                Content = dto.Comment.Trim(),
                CreatedAt = Now,
            });
        }

        await tickets.SaveChangesAsync();
    }

    public async Task AcceptAsync(int id, AcceptTicketDto dto)
    {
        var ticket = await GetVisibleAsync(id);
        var resolution = dto.Resolution.Trim();
        var today = DateOnly.FromDateTime(Now);

        if (dto.NextInspectionDate is { } next)
        {
            if (ticket.Asset is null)
                throw ValidationException.For("nextInspectionDate", "Zgłoszenie nie dotyczy urządzenia.");
            if (next < today)
                throw ValidationException.For("nextInspectionDate", "Data następnego serwisu nie może być z przeszłości.");
        }

        // Accepting a new ticket starts it first, then closes the work with the description.
        if (ticket.Status == TicketStatus.New)
        {
            TicketStatusRules.EnsureAllowed(ticket, TicketStatus.InProgress, Actor, null, null);
            Track(ticket, "Status", ticket.Status, TicketStatus.InProgress);
            ticket.Status = TicketStatus.InProgress;
        }

        TicketStatusRules.EnsureAllowed(ticket, TicketStatus.Resolved, Actor, resolution, null);
        Track(ticket, "Status", ticket.Status, TicketStatus.Resolved);
        ticket.Status = TicketStatus.Resolved;
        ticket.Resolution = resolution;
        ticket.ResolvedAt = Now;

        if (dto.NextInspectionDate is { } date && ticket.Asset is { } asset && asset.NextInspectionDate != date)
        {
            asset.History.Add(new AssetHistory
            {
                Field = "NextInspection",
                OldValue = asset.NextInspectionDate?.ToString("yyyy-MM-dd"),
                NewValue = date.ToString("yyyy-MM-dd"),
                ChangedById = currentUser.Id,
                ChangedAt = Now,
            });
            asset.NextInspectionDate = date;
        }

        await tickets.SaveChangesAsync();
    }

    public async Task AssignAsync(int id, AssignTicketDto dto)
    {
        var ticket = await GetVisibleAsync(id);
        if (ticket.Status == TicketStatus.Closed)
            throw new ConflictException("Nie można zmienić przypisania zamkniętego zgłoszenia.");

        AppUser? assignee = null;
        if (dto.AssigneeId is not null)
        {
            assignee = await userManager.FindByIdAsync(dto.AssigneeId);
            var canBeAssigned = assignee is not null
                && (await userManager.IsInRoleAsync(assignee, Roles.Moderator)
                    || await userManager.IsInRoleAsync(assignee, Roles.Administrator));
            if (!canBeAssigned)
                throw ValidationException.For("assigneeId", "Zgłoszenie można przypisać tylko technikowi lub administratorowi.");
        }

        if (ticket.AssigneeId == assignee?.Id)
            return;

        Track(ticket, "Assignee", ticket.Assignee?.Email, assignee?.Email);
        ticket.AssigneeId = assignee?.Id;
        ticket.Assignee = assignee;

        await tickets.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<CommentDto>> GetCommentsAsync(int id)
    {
        await GetVisibleAsync(id);
        var comments = await tickets.GetCommentsAsync(id);
        return comments.Select(c => c.ToDto()).ToList();
    }

    public async Task<CommentDto> AddCommentAsync(int id, CreateCommentDto dto)
    {
        var ticket = await GetVisibleAsync(id);
        var author = await userManager.FindByIdAsync(currentUser.Id) ?? throw new NotFoundException();

        var comment = new Comment
        {
            TicketId = ticket.Id,
            AuthorId = author.Id,
            Author = author,
            Content = dto.Content.Trim(),
            CreatedAt = Now,
        };
        tickets.AddComment(comment);
        await tickets.SaveChangesAsync();

        return comment.ToDto();
    }

    public async Task<IReadOnlyList<TicketHistoryDto>> GetHistoryAsync(int id)
    {
        await GetVisibleAsync(id);
        var history = await tickets.GetHistoryAsync(id);
        return history.Select(h => h.ToDto()).ToList();
    }

    private TicketActor Actor => new(currentUser.Id, currentUser.IsAdmin, currentUser.IsTechnician);

    private bool CanView(Ticket ticket) =>
        currentUser.IsAdmin
        || ticket.AuthorId == currentUser.Id
        || (currentUser.IsTechnician && ticket.AssigneeId == currentUser.Id);

    private async Task<Ticket> GetVisibleAsync(int id)
    {
        var ticket = await tickets.GetByIdAsync(id);

        // 404 for both "doesn't exist" and "not yours", so ids can't be probed.
        if (ticket is null || !CanView(ticket))
            throw new NotFoundException("Nie znaleziono zgłoszenia.");

        return ticket;
    }

    private async Task EnsureCategoryExistsAsync(int categoryId)
    {
        if (!await categories.ExistsAsync(categoryId))
            throw ValidationException.For("categoryId", "Wybrana kategoria nie istnieje.");
    }

    private async Task EnsureAssetAllowedAsync(int? assetId)
    {
        if (assetId is null)
            return;

        var asset = await assets.GetByIdAsync(assetId.Value);
        var allowed = asset is not null
            && (currentUser.IsAdmin || currentUser.IsTechnician || asset.OwnerId == currentUser.Id);

        if (!allowed)
            throw ValidationException.For("assetId", "Nieprawidłowe urządzenie.");
    }

    private void Track(Ticket ticket, string field, object? oldValue, object? newValue)
    {
        var oldText = oldValue?.ToString();
        var newText = newValue?.ToString();
        if (oldText == newText)
            return;

        ticket.History.Add(new TicketHistory
        {
            Field = field,
            OldValue = oldText,
            NewValue = newText,
            ChangedById = currentUser.Id,
            ChangedAt = Now,
        });
    }
}
