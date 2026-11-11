using Api.Auth;
using Api.DTOs;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/users")]
[Produces("application/json")]
[Authorize(Policy = Policies.Admin)]
public class UsersController(IUserService users) : ControllerBase
{
    /// <summary>All users with roles. Optional filter: ?role=Moderator</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<UserAdminDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<UserAdminDto>>> GetAll([FromQuery] string? role) =>
        Ok(await users.GetAllAsync(role));

    /// <summary>People a ticket can be assigned to (moderators and admins).</summary>
    [HttpGet("technicians")]
    [ProducesResponseType<IReadOnlyList<UserRefDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<UserRefDto>>> GetTechnicians() =>
        Ok(await users.GetTechniciansAsync());

    /// <summary>User details with address.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType<UserDetailsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDetailsDto>> Get(string id) => Ok(await users.GetDetailsAsync(id));

    /// <summary>Set or replace a user's address.</summary>
    [HttpPut("{id}/address")]
    [ProducesResponseType<UserDetailsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDetailsDto>> SetAddress(string id, AddressDto dto) =>
        Ok(await users.SetAddressAsync(id, dto));

    /// <summary>Set the user's role (replaces the current one).</summary>
    [HttpPatch("{id}/role")]
    [ProducesResponseType<UserAdminDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserAdminDto>> ChangeRole(string id, ChangeRoleDto dto) =>
        Ok(await users.ChangeRoleAsync(id, dto.Role));

    /// <summary>Lock or unlock an account. Locking logs the user out everywhere.</summary>
    [HttpPatch("{id}/lock")]
    [ProducesResponseType<UserAdminDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserAdminDto>> SetLock(string id, SetLockDto dto) =>
        Ok(await users.SetLockAsync(id, dto.Locked));
}
