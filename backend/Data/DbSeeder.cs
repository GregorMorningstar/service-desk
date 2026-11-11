using Api.Auth;
using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Api.Data;

public static class DbSeeder
{
    private const string DemoPassword = "Demo123!";
    private const string TestPassword = "Qwert12345";

    public static async Task SeedAsync(IServiceProvider services, IConfiguration config, ILogger logger)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        var userManager = services.GetRequiredService<UserManager<AppUser>>();
        var db = services.GetRequiredService<AppDbContext>();

        // Admin account from env (SeedAdmin__Email / SeedAdmin__Password).
        var adminEmail = config["SeedAdmin:Email"];
        var adminPassword = config["SeedAdmin:Password"];
        if (!string.IsNullOrWhiteSpace(adminEmail) && !string.IsNullOrWhiteSpace(adminPassword))
            await EnsureUserAsync(userManager, logger, adminEmail, adminPassword, "Administrator", "", Roles.Administrator);

        if (config.GetValue<bool>("SeedDemoData"))
            await SeedDemoAsync(db, userManager, logger);

        // Devices, tickets and categories created before barcodes existed: SaveChanges fills them in.
        await db.Assets.Where(a => a.Barcode == null).LoadAsync();
        await db.Tickets.Where(t => t.Barcode == null).LoadAsync();
        await db.AssetCategories.Where(c => c.Barcode == null).LoadAsync();
        await db.SaveChangesAsync();

        // Anyone left without a role becomes a regular user.
        var withoutRole = await db.Users
            .Where(u => !db.UserRoles.Any(ur => ur.UserId == u.Id))
            .ToListAsync();
        foreach (var user in withoutRole)
            await userManager.AddToRoleAsync(user, Roles.User);
    }

    private static async Task SeedDemoAsync(AppDbContext db, UserManager<AppUser> userManager, ILogger logger)
    {
        // Quick dev logins. The password doesn't meet the policy (no special char), so validation is skipped.
        await EnsureUserAsync(userManager, logger, "admin@test.pl", TestPassword, "Admin", "Test", Roles.Administrator, skipPasswordPolicy: true);
        await EnsureUserAsync(userManager, logger, "moderator@test.pl", TestPassword, "Moderator", "Test", Roles.Moderator, skipPasswordPolicy: true);
        var testUser = await EnsureUserAsync(userManager, logger, "user@test.pl", TestPassword, "User", "Test", Roles.User, skipPasswordPolicy: true);

        await EnsureUserAsync(userManager, logger, "admin@demo.local", DemoPassword, "Anna", "Admin", Roles.Administrator);
        var tech = await EnsureUserAsync(userManager, logger, "tech@demo.local", DemoPassword, "Tomasz", "Technik", Roles.Moderator);
        var user = await EnsureUserAsync(userManager, logger, "user@demo.local", DemoPassword, "Paweł", "Pracownik", Roles.User);
        if (tech is null || user is null || testUser is null || await db.Assets.AnyAsync())
            return;

        var laptop = new Asset
        {
            Name = "Dell Latitude 5540", CategoryId = 1, SerialNumber = "DL5540-8H2K1",
            InventoryNumber = "LAP-001", Location = "Pokój 2.14", OwnerId = user.Id,
        };
        var monitor = new Asset
        {
            Name = "Dell P2422H", CategoryId = 2, SerialNumber = "CN-0P2422H-77",
            InventoryNumber = "MON-001", Location = "Pokój 2.14", OwnerId = user.Id,
        };
        var printer = new Asset
        {
            Name = "HP LaserJet Pro M404dn", CategoryId = 3, SerialNumber = "PHBQ123456",
            InventoryNumber = "PRN-001", Location = "Korytarz, 2 piętro", OwnerId = testUser.Id,
        };
        foreach (var asset in new[] { laptop, monitor, printer })
        {
            asset.NextInspectionDate = DateOnly.FromDateTime(DateTime.UtcNow).AddMonths(asset.InspectionIntervalMonths);
            InspectionRules.SyncCard(asset);
        }
        db.Assets.AddRange(laptop, monitor, printer);
        await db.SaveChangesAsync();

        if (await db.Tickets.AnyAsync())
            return;

        var now = DateTime.UtcNow;
        var tickets = new List<Ticket>
        {
            new()
            {
                Title = "Brak połączenia z Wi-Fi",
                Description = "Laptop nie łączy się z siecią biurową od rana. Kabel działa.",
                CategoryId = 3, Priority = TicketPriority.High,
                AuthorId = user.Id, AssetId = laptop.Id, CreatedAt = now.AddHours(-3),
            },
            new()
            {
                Title = "Drukarka zacina papier",
                Description = "Drukarka na korytarzu zacina co drugą kartkę.",
                CategoryId = 1, Priority = TicketPriority.Medium,
                Status = TicketStatus.InProgress, AuthorId = user.Id, AssigneeId = tech.Id,
                AssetId = printer.Id, CreatedAt = now.AddDays(-1),
            },
            new()
            {
                Title = "Reset hasła do systemu kadrowego",
                Description = "Nie pamiętam hasła do systemu kadrowego.",
                CategoryId = 4, Priority = TicketPriority.Low,
                Status = TicketStatus.Resolved, AuthorId = user.Id, AssigneeId = tech.Id,
                Resolution = "Hasło zresetowane, nowe wysłane SMS-em.",
                CreatedAt = now.AddDays(-2), ResolvedAt = now.AddDays(-2).AddHours(1),
            },
        };

        foreach (var t in tickets)
            t.Number = Ticket.TemporaryNumber();
        db.Tickets.AddRange(tickets);
        await db.SaveChangesAsync();

        foreach (var t in tickets)
            t.Number = Ticket.FormatNumber(t.Id);
        await db.SaveChangesAsync();
    }

    private static async Task<AppUser?> EnsureUserAsync(
        UserManager<AppUser> userManager, ILogger logger,
        string email, string password, string firstName, string lastName, string role,
        bool skipPasswordPolicy = false)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new AppUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName = firstName,
                LastName = lastName,
            };

            IdentityResult result;
            if (skipPasswordPolicy)
            {
                user.PasswordHash = userManager.PasswordHasher.HashPassword(user, password);
                result = await userManager.CreateAsync(user);
            }
            else
            {
                result = await userManager.CreateAsync(user, password);
            }
            if (!result.Succeeded)
            {
                logger.LogError("Could not create {Email}: {Errors}", email,
                    string.Join("; ", result.Errors.Select(e => e.Description)));
                return null;
            }
        }

        if (!await userManager.IsInRoleAsync(user, role))
            await userManager.AddToRoleAsync(user, role);

        return user;
    }
}
