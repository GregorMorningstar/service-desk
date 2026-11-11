namespace Api.Models;

public class TicketCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public List<Ticket> Tickets { get; set; } = [];
}
