using Api.Auth;
using Api.Common;
using Api.DTOs;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <remarks>
/// Visibility: User sees own tickets, Moderator (technician) own + assigned, Administrator all.
/// Tickets you can't see return 404.
/// </remarks>
[ApiController]
[Route("api/v1/tickets")]
[Produces("application/json")]
public class TicketsController(ITicketService tickets) : ControllerBase
{
    /// <summary>Paged ticket list with filters.</summary>
    [HttpGet]
    [ProducesResponseType<PagedResult<TicketListItemDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<TicketListItemDto>>> GetList([FromQuery] TicketListQueryDto query) =>
        Ok(await tickets.GetListAsync(query));

    /// <summary>Ticket details.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<TicketDetailsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketDetailsDto>> Get(int id) => Ok(await tickets.GetAsync(id));

    /// <summary>Create a ticket. Status starts as New.</summary>
    [HttpPost]
    [ProducesResponseType<TicketDetailsDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TicketDetailsDto>> Create(SaveTicketDto dto)
    {
        var created = await tickets.CreateAsync(dto);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    /// <summary>Edit title, description, category, priority, device. Author or admin.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType<TicketDetailsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TicketDetailsDto>> Update(int id, SaveTicketDto dto) =>
        Ok(await tickets.UpdateAsync(id, dto));

    /// <summary>Change status.</summary>
    /// <remarks>
    /// New → InProgress: assigned technician or admin (ticket must have an assignee).
    /// InProgress → Resolved: assigned technician or admin, `resolution` required.
    /// Resolved → Closed: author or admin.
    /// Resolved → InProgress (reopen): author or admin, `comment` required.
    /// Anything else returns 409.
    /// </remarks>
    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ChangeStatus(int id, ChangeStatusDto dto)
    {
        await tickets.ChangeStatusAsync(id, dto);
        return NoContent();
    }

    /// <summary>
    /// Finish the work: New or InProgress → Resolved with a description of the changes made.
    /// For a ticket with a device the next service date can be set. Assigned technician or admin.
    /// </summary>
    [HttpPost("{id:int}/accept")]
    [Authorize(Policy = Policies.Staff)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Accept(int id, AcceptTicketDto dto)
    {
        await tickets.AcceptAsync(id, dto);
        return NoContent();
    }

    /// <summary>Assign a technician (or null to unassign). Admin only.</summary>
    [HttpPatch("{id:int}/assignee")]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Assign(int id, AssignTicketDto dto)
    {
        await tickets.AssignAsync(id, dto);
        return NoContent();
    }

    /// <summary>Ticket comments, oldest first.</summary>
    [HttpGet("{id:int}/comments")]
    [ProducesResponseType<IReadOnlyList<CommentDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<CommentDto>>> GetComments(int id) =>
        Ok(await tickets.GetCommentsAsync(id));

    /// <summary>Add a comment.</summary>
    [HttpPost("{id:int}/comments")]
    [ProducesResponseType<CommentDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CommentDto>> AddComment(int id, CreateCommentDto dto)
    {
        var comment = await tickets.AddCommentAsync(id, dto);
        return CreatedAtAction(nameof(GetComments), new { id }, comment);
    }

    /// <summary>Change log: status, assignee, title, category, priority, device.</summary>
    [HttpGet("{id:int}/history")]
    [ProducesResponseType<IReadOnlyList<TicketHistoryDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<TicketHistoryDto>>> GetHistory(int id) =>
        Ok(await tickets.GetHistoryAsync(id));
}
