using Api.Data;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Repositories;

public record TicketCategoryWithCount(TicketCategory Category, int TicketCount);

public interface ITicketCategoryRepository
{
    Task<List<TicketCategoryWithCount>> GetAllAsync(string? search = null);
    Task<TicketCategory?> GetByIdAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> NameExistsAsync(string name, int? exceptId = null);
    Task<int> CountTicketsAsync(int categoryId);
    void Add(TicketCategory category);
    void Remove(TicketCategory category);
    Task SaveChangesAsync();
}

public class TicketCategoryRepository(AppDbContext db) : ITicketCategoryRepository
{
    public Task<List<TicketCategoryWithCount>> GetAllAsync(string? search = null)
    {
        var query = db.TicketCategories.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(c => c.Name.Contains(term));
        }

        return query
            .OrderBy(c => c.Name)
            .Select(c => new TicketCategoryWithCount(c, c.Tickets.Count))
            .ToListAsync();
    }

    public Task<TicketCategory?> GetByIdAsync(int id) => db.TicketCategories.FirstOrDefaultAsync(c => c.Id == id);

    public Task<bool> ExistsAsync(int id) => db.TicketCategories.AnyAsync(c => c.Id == id);

    public Task<bool> NameExistsAsync(string name, int? exceptId = null) =>
        db.TicketCategories.AnyAsync(c => c.Name == name && c.Id != exceptId);

    public Task<int> CountTicketsAsync(int categoryId) => db.Tickets.CountAsync(t => t.CategoryId == categoryId);

    public void Add(TicketCategory category) => db.TicketCategories.Add(category);

    public void Remove(TicketCategory category) => db.TicketCategories.Remove(category);

    public Task SaveChangesAsync() => db.SaveChangesAsync();
}
