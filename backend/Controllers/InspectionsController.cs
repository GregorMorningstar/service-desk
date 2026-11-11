using Api.Auth;
using Api.DTOs;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/inspections")]
[Produces("application/json")]
public class InspectionsController(IInspectionService inspections) : ControllerBase
{
    /// <summary>
    /// Inspection cards: device, address, owner and due date, soonest first. Optional ?assetId=.
    /// Staff see all cards, regular users only the cards of their own devices.
    /// </summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<InspectionDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<InspectionDto>>> GetAll([FromQuery] int? assetId) =>
        Ok(await inspections.GetAllAsync(assetId));

    /// <summary>One card with its inspections and failure handling.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<InspectionDetailsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InspectionDetailsDto>> Get(int id) => Ok(await inspections.GetAsync(id));

    /// <summary>Records waiting for approval. Admin only.</summary>
    [HttpGet("records/pending")]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType<IReadOnlyList<PendingInspectionRecordDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PendingInspectionRecordDto>>> GetPending() =>
        Ok(await inspections.GetPendingAsync());

    /// <summary>
    /// Add an inspection (date, cost, next inspection) or a failure handling to a card.
    /// An admin's entry is approved at once; a technician's waits for an admin.
    /// </summary>
    [HttpPost("{id:int}/records")]
    [Authorize(Policy = Policies.Staff)]
    [ProducesResponseType<InspectionRecordDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InspectionRecordDto>> AddRecord(int id, SaveInspectionRecordDto dto)
    {
        var created = await inspections.AddRecordAsync(id, dto);
        return CreatedAtAction(nameof(Get), new { id }, created);
    }

    /// <summary>Edit a record. Admin only.</summary>
    [HttpPut("records/{recordId:int}")]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType<InspectionRecordDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InspectionRecordDto>> UpdateRecord(int recordId, SaveInspectionRecordDto dto) =>
        Ok(await inspections.UpdateRecordAsync(recordId, dto));

    /// <summary>Delete a record. Admin only.</summary>
    [HttpDelete("records/{recordId:int}")]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRecord(int recordId)
    {
        await inspections.DeleteRecordAsync(recordId);
        return NoContent();
    }

    /// <summary>Approve or reject a pending record: { "approve": true }. Admin only.</summary>
    [HttpPost("records/{recordId:int}/review")]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType<InspectionRecordDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<InspectionRecordDto>> Review(int recordId, ReviewInspectionRecordDto dto) =>
        Ok(await inspections.ReviewAsync(recordId, dto));
}
