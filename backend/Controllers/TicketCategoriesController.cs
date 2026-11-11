using Api.Auth;
using Api.DTOs;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/ticket-categories")]
[Produces("application/json")]
public class TicketCategoriesController(ITicketCategoryService categories) : ControllerBase
{
    /// <summary>Ticket categories with the number of tickets in each. Optional search by name. Any logged-in user.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<TicketCategoryDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TicketCategoryDto>>> GetAll([FromQuery] string? search = null) =>
        Ok(await categories.GetAllAsync(search));

    /// <summary>Single category.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<TicketCategoryDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketCategoryDto>> Get(int id) => Ok(await categories.GetAsync(id));

    /// <summary>Add a category. Admin only.</summary>
    [HttpPost]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType<TicketCategoryDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TicketCategoryDto>> Create(SaveTicketCategoryDto dto)
    {
        var created = await categories.CreateAsync(dto);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    /// <summary>Rename / change description. Admin only.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType<TicketCategoryDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TicketCategoryDto>> Update(int id, SaveTicketCategoryDto dto) =>
        Ok(await categories.UpdateAsync(id, dto));

    /// <summary>Delete a category. Fails with 409 while any ticket uses it. Admin only.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
        await categories.DeleteAsync(id);
        return NoContent();
    }
}
