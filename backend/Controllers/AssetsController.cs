using Api.Auth;
using Api.DTOs;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/assets")]
[Produces("application/json")]
public class AssetsController(IAssetService assets) : ControllerBase
{
    /// <summary>
    /// Device list. Filters: categoryId, search, userId (staff also: owner, address, isActive, status).
    /// Regular users always get only their own devices (userId is ignored for them).
    /// </summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<AssetDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AssetDto>>> GetAll([FromQuery] AssetQueryDto query) =>
        Ok(await assets.GetAllAsync(query));

    /// <summary>
    /// Addresses linked to a user (their own and those of their devices) to pick from when adding a device.
    /// Staff can pass userId; everyone else gets their own.
    /// </summary>
    [HttpGet("addresses")]
    [ProducesResponseType<IReadOnlyList<KnownAddressDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<KnownAddressDto>>> KnownAddresses([FromQuery] KnownAddressQueryDto query) =>
        Ok(await assets.GetKnownAddressesAsync(query));

    /// <summary>Tells whether a serial or inventory number is already used by another device (for live form checks).</summary>
    [HttpGet("availability")]
    [ProducesResponseType<AssetAvailabilityDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<AssetAvailabilityDto>> Availability([FromQuery] AssetAvailabilityQueryDto query) =>
        Ok(await assets.CheckAvailabilityAsync(query));

    /// <summary>Device details.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<AssetDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssetDto>> Get(int id) => Ok(await assets.GetAsync(id));

    /// <summary>Change log of a device: owner, category, address, location, numbers. Owner, technician or admin.</summary>
    [HttpGet("{id:int}/history")]
    [ProducesResponseType<IReadOnlyList<AssetHistoryDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<AssetHistoryDto>>> GetHistory(int id) =>
        Ok(await assets.GetHistoryAsync(id));

    /// <summary>Tickets linked to a device. Technician or admin.</summary>
    [HttpGet("{id:int}/tickets")]
    [Authorize(Policy = Policies.Staff)]
    [ProducesResponseType<IReadOnlyList<TicketListItemDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<TicketListItemDto>>> GetTickets(int id) =>
        Ok(await assets.GetTicketsAsync(id));

    /// <summary>
    /// Propose a new device. It is assigned to the caller and waits for an admin's approval
    /// (admin's own devices are approved at once).
    /// </summary>
    [HttpPost]
    [ProducesResponseType<AssetDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AssetDto>> Request(RequestAssetDto dto)
    {
        var created = await assets.RequestAsync(dto);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }
}
