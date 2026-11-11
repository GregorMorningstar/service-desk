using Api.Data;

namespace Api.Models;

public class Comment
{
    public int Id { get; set; }

    public int TicketId { get; set; }
    public Ticket? Ticket { get; set; }

    public string AuthorId { get; set; } = string.Empty;
    public AppUser? Author { get; set; }

    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
