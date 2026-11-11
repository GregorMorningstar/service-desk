using System.ComponentModel.DataAnnotations;
using Api.Models;

namespace Api.DTOs;

public record TicketCategoryRefDto(int Id, string Name);

public record TicketCategoryDto(int Id, string Name, string? Description, int TicketCount);

public record SaveTicketCategoryDto
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    [StringLength(255)]
    public string? Description { get; init; }
}

public record TicketListItemDto(
    int Id,
    string Number,
    string Title,
    TicketStatus Status,
    TicketPriority Priority,
    TicketCategoryRefDto Category,
    UserRefDto Author,
    UserRefDto? Assignee,
    DateTime CreatedAt,
    string? Barcode,
    AssetRefDto? Asset);

public record AssetRefDto(int Id, string Name, string InventoryNumber);

public record TicketDetailsDto(
    int Id,
    string Number,
    string Title,
    string Description,
    TicketStatus Status,
    TicketPriority Priority,
    TicketCategoryRefDto Category,
    UserRefDto Author,
    UserRefDto? Assignee,
    AssetRefDto? Asset,
    string? Resolution,
    DateTime CreatedAt,
    DateTime? ResolvedAt,
    string? Barcode);

public record TicketListQueryDto
{
    public TicketStatus? Status { get; init; }
    public TicketPriority? Priority { get; init; }
    public int? CategoryId { get; init; }

    /// <summary>Searches number, title and description.</summary>
    [StringLength(100)]
    public string? Search { get; init; }

    public string? AssigneeId { get; init; }

    /// <summary>Only tickets assigned to the caller (a technician's repair history). Overrides assigneeId.</summary>
    public bool AssignedToMe { get; init; }

    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;

    [Range(1, 100)]
    public int PageSize { get; init; } = 20;
}

public record SaveTicketDto
{
    [Required, StringLength(150, MinimumLength = 3)]
    public string Title { get; init; } = string.Empty;

    [Required, StringLength(10000)]
    public string Description { get; init; } = string.Empty;

    [Required]
    public int? CategoryId { get; init; }

    [Required]
    public TicketPriority? Priority { get; init; }

    public int? AssetId { get; init; }
}

public record ChangeStatusDto
{
    [Required]
    public TicketStatus? Status { get; init; }

    /// <summary>Required when moving to Resolved.</summary>
    [StringLength(10000)]
    public string? Resolution { get; init; }

    /// <summary>Required when reopening (Resolved → InProgress). Saved as a comment.</summary>
    [StringLength(4000)]
    public string? Comment { get; init; }
}

/// <summary>The technician finishes the job: describes what was changed and, for a service, sets the next date.</summary>
public record AcceptTicketDto
{
    [Required, StringLength(10000, MinimumLength = 3)]
    public string Resolution { get; init; } = string.Empty;

    /// <summary>Next service of the linked device. Only for tickets that have a device.</summary>
    public DateOnly? NextInspectionDate { get; init; }
}

public record AssignTicketDto
{
    /// <summary>Technician or admin id; null removes the assignment.</summary>
    public string? AssigneeId { get; init; }
}

public record CommentDto(int Id, string Content, UserRefDto Author, DateTime CreatedAt);

public record CreateCommentDto
{
    [Required, StringLength(4000, MinimumLength = 1)]
    public string Content { get; init; } = string.Empty;
}

public record TicketHistoryDto(
    int Id,
    string Field,
    string? OldValue,
    string? NewValue,
    UserRefDto ChangedBy,
    DateTime ChangedAt);
