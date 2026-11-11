using Api.DTOs;
using Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>Photos of a device. Owner, technician or admin can view; owner or admin can change.</summary>
[ApiController]
[Route("api/v1/assets/{assetId:int}/photos")]
[Produces("application/json")]
public class AssetPhotosController(IAssetPhotoService photos) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<AssetPhotoDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<AssetPhotoDto>>> GetAll(int assetId) =>
        Ok(await photos.GetAllAsync(assetId));

    /// <summary>Upload one photo (multipart field "file"): JPEG, PNG or WebP, up to 5 MB, 10 per device.</summary>
    [HttpPost]
    [RequestSizeLimit(AssetPhotoService.MaxBytes + 64 * 1024)]
    [ProducesResponseType<AssetPhotoDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AssetPhotoDto>> Upload(int assetId, IFormFile file, CancellationToken ct)
    {
        var created = await photos.AddAsync(assetId, file, ct);
        return CreatedAtAction(nameof(Download), new { assetId, photoId = created.Id }, created);
    }

    /// <summary>The image file itself.</summary>
    [HttpGet("{photoId:int}")]
    [Produces("image/jpeg", "image/png", "image/webp")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Download(int assetId, int photoId)
    {
        var (content, contentType) = await photos.GetFileAsync(assetId, photoId);
        Response.Headers.CacheControl = "private, max-age=3600";
        return File(content, contentType);
    }

    [HttpDelete("{photoId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int assetId, int photoId)
    {
        await photos.DeleteAsync(assetId, photoId);
        return NoContent();
    }
}
