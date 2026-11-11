using Api.Data;

namespace Api.Models;

public class Ticket : IHasBarcode
{
    public int Id { get; set; }

    /// <summary>EAN-13, filled after the first save.</summary>
    public string? Barcode { get; set; }

    /// <summary>Business number, e.g. INC-000123.</summary>
    public string Number { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public TicketCategory? Category { get; set; }
    public TicketPriority Priority { get; set; }
    public TicketStatus Status { get; set; } = TicketStatus.New;

    public string AuthorId { get; set; } = string.Empty;
    public AppUser? Author { get; set; }

    public string? AssigneeId { get; set; }
    public AppUser? Assignee { get; set; }

    public int? AssetId { get; set; }
    public Asset? Asset { get; set; }

    public string? Resolution { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }

    public List<Comment> Comments { get; set; } = [];
    public List<TicketHistory> History { get; set; } = [];

    public static string FormatNumber(int id) => $"INC-{id:D6}";

    // Placeholder that satisfies the unique index until the real Id is known.
    public static string TemporaryNumber() => "TMP-" + Guid.NewGuid().ToString("N")[..16];
}
