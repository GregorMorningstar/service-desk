using Api.Auth;
using Api.DTOs;
using Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>The logged-in user's own data.</summary>
[ApiController]
[Route("api/v1/profile")]
[Produces("application/json")]
public class ProfileController(IUserService users, ICurrentUser currentUser) : ControllerBase
{
    /// <summary>My profile with address.</summary>
    [HttpGet]
    [ProducesResponseType<UserDetailsDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<UserDetailsDto>> Get() => Ok(await users.GetDetailsAsync(currentUser.Id));

    /// <summary>Set or replace my address.</summary>
    [HttpPut("address")]
    [ProducesResponseType<UserDetailsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserDetailsDto>> SetAddress(AddressDto dto) =>
        Ok(await users.SetAddressAsync(currentUser.Id, dto));

    /// <summary>Remove my address.</summary>
    [HttpDelete("address")]
    [ProducesResponseType<UserDetailsDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<UserDetailsDto>> DeleteAddress() =>
        Ok(await users.SetAddressAsync(currentUser.Id, null));
}
