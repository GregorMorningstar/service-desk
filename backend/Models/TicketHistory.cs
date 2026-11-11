using Api.Data;

namespace Api.Models;

public class TicketHistory
{
    public int Id { get; set; }

    public int TicketId { get; set; }
    public Ticket? Ticket { get; set; }

    public string ChangedById { get; set; } = string.Empty;
    public AppUser? ChangedBy { get; set; }

    public string Field { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public DateTime ChangedAt { get; set; }
}
