using Api.Auth;
using Api.DTOs;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/dashboard")]
[Produces("application/json")]
public class DashboardController(IDashboardService dashboard) : ControllerBase
{
    /// <summary>
    /// Start page of the admin and the moderator: nearest services and the most urgent open tickets.
    /// A moderator gets only the tickets assigned to them.
    /// </summary>
    [HttpGet("overview")]
    [Authorize(Policy = Policies.Staff)]
    [ProducesResponseType<AdminOverviewDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<AdminOverviewDto>> GetOverview() => Ok(await dashboard.GetOverviewAsync());

    /// <summary>Ticket counts by status, priority and technician. Admin only.</summary>
    [HttpGet("stats")]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType<DashboardStatsDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardStatsDto>> GetStats() => Ok(await dashboard.GetStatsAsync());
}
