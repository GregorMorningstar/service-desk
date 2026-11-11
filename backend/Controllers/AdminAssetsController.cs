using Api.Auth;
using Api.DTOs;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>Full device management. Admin only; users work through /api/v1/assets.</summary>
[ApiController]
[Route("api/v1/admin/assets")]
[Authorize(Policy = Policies.Admin)]
[Produces("application/json")]
public class AdminAssetsController(IAssetService assets) : ControllerBase
{
    /// <summary>All devices. Filters: search (name, serial, inventory no.), owner (name or e-mail), userId, address, categoryId, isActive, status.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<AssetDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AssetDto>>> GetAll([FromQuery] AssetQueryDto query) =>
        Ok(await assets.GetAllAsync(query));

    [HttpGet("{id:int}")]
    [ProducesResponseType<AssetDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssetDto>> Get(int id) => Ok(await assets.GetAsync(id));

    /// <summary>Add a device for any user. Approved at once.</summary>
    [HttpPost]
    [ProducesResponseType<AssetDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AssetDto>> Create(SaveAssetDto dto)
    {
        var created = await assets.CreateAsync(dto);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    /// <summary>Edit a device, including owner and the active flag.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType<AssetDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AssetDto>> Update(int id, SaveAssetDto dto) =>
        Ok(await assets.UpdateAsync(id, dto));

    /// <summary>Delete a device. Linked tickets stay, without the device.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        await assets.DeleteAsync(id);
        return NoContent();
    }

    /// <summary>Switch a device on or off.</summary>
    [HttpPatch("{id:int}/active")]
    [ProducesResponseType<AssetDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AssetDto>> SetActive(int id, SetActiveDto dto) =>
        Ok(await assets.SetActiveAsync(id, dto.IsActive));

    /// <summary>Approve a device added by a user.</summary>
    [HttpPost("{id:int}/approve")]
    [ProducesResponseType<AssetDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AssetDto>> Approve(int id) => Ok(await assets.ReviewAsync(id, approve: true));

    /// <summary>Reject a device added by a user.</summary>
    [HttpPost("{id:int}/reject")]
    [ProducesResponseType<AssetDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AssetDto>> Reject(int id) => Ok(await assets.ReviewAsync(id, approve: false));
}
