using Api.Common;
using Api.Models;

namespace Api.Services;

public record TicketActor(string UserId, bool IsAdmin, bool IsTechnician);

/// <summary>
/// Ticket lifecycle from the spec:
/// New → InProgress → Resolved → Closed, and Resolved → InProgress (reopen).
/// </summary>
public static class TicketStatusRules
{
    public static void EnsureAllowed(
        Ticket ticket, TicketStatus target, TicketActor actor, string? resolution, string? comment)
    {
        var isAuthor = ticket.AuthorId == actor.UserId;
        var isAssignee = ticket.AssigneeId is not null && ticket.AssigneeId == actor.UserId;

        switch ((ticket.Status, target))
        {
            case (TicketStatus.New, TicketStatus.InProgress):
                Require(actor.IsAdmin || (actor.IsTechnician && isAssignee));
                if (ticket.AssigneeId is null)
                    throw new ConflictException("Najpierw przypisz technika do zgłoszenia.");
                break;

            case (TicketStatus.InProgress, TicketStatus.Resolved):
                Require(actor.IsAdmin || isAssignee);
                if (string.IsNullOrWhiteSpace(resolution))
                    throw ValidationException.For("resolution", "Opis rozwiązania jest wymagany.");
                break;

            case (TicketStatus.Resolved, TicketStatus.Closed):
                Require(actor.IsAdmin || isAuthor);
                break;

            case (TicketStatus.Resolved, TicketStatus.InProgress):
                Require(actor.IsAdmin || isAuthor);
                if (string.IsNullOrWhiteSpace(comment))
                    throw ValidationException.For("comment", "Podaj powód ponownego otwarcia zgłoszenia.");
                break;

            default:
                throw new ConflictException($"Niedozwolone przejście statusu: {ticket.Status} → {target}.");
        }
    }

    private static void Require(bool allowed)
    {
        if (!allowed)
            throw new ForbiddenException("Nie możesz wykonać tej zmiany statusu.");
    }
}
