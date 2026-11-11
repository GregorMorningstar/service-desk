using Api.Common;
using Api.DTOs;
using Api.Models;
using Api.Repositories;

namespace Api.Services;

public interface ITicketCategoryService
{
    Task<IReadOnlyList<TicketCategoryDto>> GetAllAsync(string? search = null);
    Task<TicketCategoryDto> GetAsync(int id);
    Task<TicketCategoryDto> CreateAsync(SaveTicketCategoryDto dto);
    Task<TicketCategoryDto> UpdateAsync(int id, SaveTicketCategoryDto dto);
    Task DeleteAsync(int id);
}

public class TicketCategoryService(ITicketCategoryRepository categories) : ITicketCategoryService
{
    public async Task<IReadOnlyList<TicketCategoryDto>> GetAllAsync(string? search = null)
    {
        var list = await categories.GetAllAsync(search);
        return list.Select(c => ToDto(c.Category, c.TicketCount)).ToList();
    }

    public async Task<TicketCategoryDto> GetAsync(int id)
    {
        var category = await FindAsync(id);
        return ToDto(category, await categories.CountTicketsAsync(id));
    }

    public async Task<TicketCategoryDto> CreateAsync(SaveTicketCategoryDto dto)
    {
        var name = dto.Name.Trim();
        if (await categories.NameExistsAsync(name))
            throw new ConflictException("Kategoria o tej nazwie już istnieje.");

        var category = new TicketCategory { Name = name, Description = Clean(dto.Description) };
        categories.Add(category);
        await categories.SaveChangesAsync();

        return ToDto(category, 0);
    }

    public async Task<TicketCategoryDto> UpdateAsync(int id, SaveTicketCategoryDto dto)
    {
        var category = await FindAsync(id);
        var name = dto.Name.Trim();
        if (await categories.NameExistsAsync(name, exceptId: id))
            throw new ConflictException("Kategoria o tej nazwie już istnieje.");

        category.Name = name;
        category.Description = Clean(dto.Description);
        await categories.SaveChangesAsync();

        return ToDto(category, await categories.CountTicketsAsync(id));
    }

    public async Task DeleteAsync(int id)
    {
        var category = await FindAsync(id);
        var inUse = await categories.CountTicketsAsync(id);
        if (inUse > 0)
            throw new ConflictException($"Kategoria jest używana przez {inUse} zgłoszeń. Najpierw zmień im kategorię.");

        categories.Remove(category);
        await categories.SaveChangesAsync();
    }

    private async Task<TicketCategory> FindAsync(int id) =>
        await categories.GetByIdAsync(id) ?? throw new NotFoundException("Nie znaleziono kategorii.");

    private static TicketCategoryDto ToDto(TicketCategory c, int ticketCount) =>
        new(c.Id, c.Name, c.Description, ticketCount);

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
