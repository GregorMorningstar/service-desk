using Api.Auth;
using Api.DTOs;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/asset-categories")]
[Produces("application/json")]
public class AssetCategoriesController(IAssetCategoryService categories) : ControllerBase
{
    /// <summary>Device categories with the number of devices in each. Optional search by name. Any logged-in user.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<AssetCategoryDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AssetCategoryDto>>> GetAll([FromQuery] string? search = null) =>
        Ok(await categories.GetAllAsync(search));

    /// <summary>Single category.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<AssetCategoryDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssetCategoryDto>> Get(int id) => Ok(await categories.GetAsync(id));

    /// <summary>Tickets of the devices in a category (any status), newest first. Technician or admin.</summary>
    [HttpGet("{id:int}/tickets")]
    [Authorize(Policy = Policies.Staff)]
    [ProducesResponseType<IReadOnlyList<TicketListItemDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<TicketListItemDto>>> GetTickets(int id) =>
        Ok(await categories.GetTicketsAsync(id));

    /// <summary>Add a category. Admin only.</summary>
    [HttpPost]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType<AssetCategoryDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AssetCategoryDto>> Create(SaveAssetCategoryDto dto)
    {
        var created = await categories.CreateAsync(dto);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    /// <summary>Rename / change description. Admin only.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType<AssetCategoryDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AssetCategoryDto>> Update(int id, SaveAssetCategoryDto dto) =>
        Ok(await categories.UpdateAsync(id, dto));

    /// <summary>Delete a category. Fails with 409 while any device uses it. Admin only.</summary>
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
