using Api.Data;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Repositories;

public interface IAssetPhotoRepository
{
    Task<List<AssetPhoto>> GetByAssetAsync(int assetId);
    Task<AssetPhoto?> GetAsync(int assetId, int photoId);
    Task<int> CountAsync(int assetId);
    void Add(AssetPhoto photo);
    void Remove(AssetPhoto photo);
    Task SaveChangesAsync();
}

public class AssetPhotoRepository(AppDbContext db) : IAssetPhotoRepository
{
    public Task<List<AssetPhoto>> GetByAssetAsync(int assetId) =>
        db.AssetPhotos.AsNoTracking().Where(p => p.AssetId == assetId).OrderBy(p => p.Id).ToListAsync();

    public Task<AssetPhoto?> GetAsync(int assetId, int photoId) =>
        db.AssetPhotos.FirstOrDefaultAsync(p => p.AssetId == assetId && p.Id == photoId);

    public Task<int> CountAsync(int assetId) => db.AssetPhotos.CountAsync(p => p.AssetId == assetId);

    public void Add(AssetPhoto photo) => db.AssetPhotos.Add(photo);

    public void Remove(AssetPhoto photo) => db.AssetPhotos.Remove(photo);

    public Task SaveChangesAsync() => db.SaveChangesAsync();
}
