using Api.Auth;
using Api.Common;
using Api.DTOs;
using Api.Models;
using Api.Repositories;

namespace Api.Services;

public interface IAssetPhotoService
{
    Task<IReadOnlyList<AssetPhotoDto>> GetAllAsync(int assetId);
    Task<AssetPhotoDto> AddAsync(int assetId, IFormFile file, CancellationToken ct);
    Task<(Stream Content, string ContentType)> GetFileAsync(int assetId, int photoId);
    Task DeleteAsync(int assetId, int photoId);
}

/// <summary>Photos of a device. Visible to whoever sees the device; changed only by its owner or an admin.</summary>
public class AssetPhotoService(
    IAssetService assets,
    IAssetPhotoRepository photos,
    IPhotoStorage storage,
    ICurrentUser currentUser,
    TimeProvider time) : IAssetPhotoService
{
    public const int MaxBytes = 5 * 1024 * 1024;
    public const int MaxPerAsset = 10;

    public async Task<IReadOnlyList<AssetPhotoDto>> GetAllAsync(int assetId)
    {
        await assets.GetAsync(assetId); // 404 when the device is not visible
        return (await photos.GetByAssetAsync(assetId)).Select(ToDto).ToList();
    }

    public async Task<AssetPhotoDto> AddAsync(int assetId, IFormFile file, CancellationToken ct)
    {
        await EnsureCanChangeAsync(assetId);

        if (file.Length == 0)
            throw ValidationException.For("file", "Plik jest pusty.");
        if (file.Length > MaxBytes)
            throw ValidationException.For("file", "Zdjęcie może mieć maksymalnie 5 MB.");
        if (await photos.CountAsync(assetId) >= MaxPerAsset)
            throw new ConflictException($"Urządzenie może mieć maksymalnie {MaxPerAsset} zdjęć.");

        await using var content = file.OpenReadStream();
        var (contentType, extension) = await DetectAsync(content, ct)
            ?? throw ValidationException.For("file", "Dozwolone formaty: JPEG, PNG, WebP.");

        var photo = new AssetPhoto
        {
            AssetId = assetId,
            StoredName = Guid.NewGuid().ToString("N") + extension,
            OriginalName = Path.GetFileName(file.FileName),
            ContentType = contentType,
            Size = file.Length,
            UploadedAt = time.GetUtcNow().UtcDateTime,
            UploadedById = currentUser.Id,
        };

        content.Position = 0;
        await storage.SaveAsync(assetId, photo.StoredName, content, ct);

        photos.Add(photo);
        await photos.SaveChangesAsync();
        return ToDto(photo);
    }

    public async Task<(Stream Content, string ContentType)> GetFileAsync(int assetId, int photoId)
    {
        await assets.GetAsync(assetId);
        var photo = await photos.GetAsync(assetId, photoId) ?? throw new NotFoundException("Nie znaleziono zdjęcia.");
        var stream = storage.OpenRead(assetId, photo.StoredName) ?? throw new NotFoundException("Nie znaleziono zdjęcia.");
        return (stream, photo.ContentType);
    }

    public async Task DeleteAsync(int assetId, int photoId)
    {
        await EnsureCanChangeAsync(assetId);
        var photo = await photos.GetAsync(assetId, photoId) ?? throw new NotFoundException("Nie znaleziono zdjęcia.");

        photos.Remove(photo);
        await photos.SaveChangesAsync();
        storage.Delete(assetId, photo.StoredName);
    }

    private async Task EnsureCanChangeAsync(int assetId)
    {
        var asset = await assets.GetAsync(assetId);
        if (!currentUser.IsAdmin && asset.Owner?.Id != currentUser.Id)
            throw new NotFoundException("Nie znaleziono urządzenia.");
    }

    /// <summary>Recognises the format by its first bytes, so a renamed file is not accepted.</summary>
    private static async Task<(string ContentType, string Extension)?> DetectAsync(Stream s, CancellationToken ct)
    {
        var head = new byte[12];
        var read = await s.ReadAtLeastAsync(head, head.Length, throwOnEndOfStream: false, ct);
        if (read < 12) return null;

        if (head[0] == 0xFF && head[1] == 0xD8 && head[2] == 0xFF) return ("image/jpeg", ".jpg");
        if (head[0] == 0x89 && head[1] == 0x50 && head[2] == 0x4E && head[3] == 0x47) return ("image/png", ".png");
        if (head[0] == 'R' && head[1] == 'I' && head[2] == 'F' && head[3] == 'F' &&
            head[8] == 'W' && head[9] == 'E' && head[10] == 'B' && head[11] == 'P') return ("image/webp", ".webp");
        return null;
    }

    private static AssetPhotoDto ToDto(AssetPhoto p) => new(p.Id, p.OriginalName, p.ContentType, p.Size, p.UploadedAt);
}
