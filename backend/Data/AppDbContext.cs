using Api.Common;
using Api.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Options;

namespace Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options, IOptions<BarcodeOptions> barcodes)
    : IdentityDbContext<AppUser>(options)
{
    public DbSet<AssetPhoto> AssetPhotos => Set<AssetPhoto>();

    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<TicketHistory> TicketHistory => Set<TicketHistory>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<AssetCategory> AssetCategories => Set<AssetCategory>();
    public DbSet<AssetHistory> AssetHistory => Set<AssetHistory>();
    public DbSet<TicketCategory> TicketCategories => Set<TicketCategory>();
    public DbSet<Inspection> Inspections => Set<Inspection>();
    public DbSet<InspectionRecord> InspectionRecords => Set<InspectionRecord>();

    /// <summary>Saves, then gives every new device, ticket and category its barcode (the id is only known now).</summary>
    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

        var missing = ChangeTracker.Entries<IHasBarcode>().Where(e => e.Entity.Barcode is null).ToList();
        if (missing.Count == 0)
            return result;

        var o = barcodes.Value;
        foreach (var entry in missing)
        {
            var prefix = entry.Entity switch
            {
                Asset => o.AssetPrefix,
                Ticket => o.TicketPrefix,
                _ => o.AssetCategoryPrefix,
            };
            entry.Entity.Barcode = Barcodes.Create(prefix, entry.Entity.Id);
        }

        return result + await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder builder)
    {
        // MySQL can't index unbounded text, so strings default to varchar(255).
        // Long text columns override this below.
        builder.Properties<string>().HaveMaxLength(255);

        // The MySQL provider hands `date` columns back as DateTime, so DateOnly needs a converter.
        builder.Properties<DateOnly>().HaveConversion<DateOnlyConverter>().HaveColumnType("date");
    }

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<Ticket>(e =>
        {
            e.Property(t => t.Number).HasMaxLength(20);
            e.Property(t => t.Barcode).HasMaxLength(13);
            e.HasIndex(t => t.Barcode).IsUnique();
            e.HasIndex(t => t.Number).IsUnique();
            e.Property(t => t.Title).HasMaxLength(150);
            e.Property(t => t.Description).HasColumnType("longtext");
            e.Property(t => t.Resolution).HasColumnType("longtext");
            e.HasIndex(t => t.Status);
            e.HasIndex(t => t.CreatedAt);

            e.HasOne(t => t.Author).WithMany().HasForeignKey(t => t.AuthorId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(t => t.Assignee).WithMany().HasForeignKey(t => t.AssigneeId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(t => t.Asset).WithMany(a => a.Tickets).HasForeignKey(t => t.AssetId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(t => t.Category).WithMany(c => c.Tickets).HasForeignKey(t => t.CategoryId).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<TicketCategory>(e =>
        {
            e.Property(c => c.Name).HasMaxLength(100);
            e.HasIndex(c => c.Name).IsUnique();
            e.Property(c => c.Description).HasMaxLength(255);

            // Ids match the old TicketCategory enum (+1) so the migration can map existing tickets.
            e.HasData(
                new TicketCategory { Id = 1, Name = "Sprzęt" },
                new TicketCategory { Id = 2, Name = "Oprogramowanie" },
                new TicketCategory { Id = 3, Name = "Sieć" },
                new TicketCategory { Id = 4, Name = "Konto" },
                new TicketCategory { Id = 5, Name = "Inne" });
        });

        b.Entity<Asset>(e =>
        {
            e.Property(a => a.Name).HasMaxLength(100);
            e.Property(a => a.SerialNumber).HasMaxLength(50);
            e.HasIndex(a => a.SerialNumber).IsUnique();
            e.Property(a => a.InventoryNumber).HasMaxLength(30);
            e.HasIndex(a => a.InventoryNumber).IsUnique();
            e.Property(a => a.Location).HasMaxLength(100);
            e.Property(a => a.Description).HasMaxLength(500);
            e.Property(a => a.Barcode).HasMaxLength(13);
            e.HasIndex(a => a.Barcode).IsUnique();
            e.Property(a => a.ApprovalStatus).HasConversion<string>().HasMaxLength(20);
            e.HasIndex(a => a.ApprovalStatus);
            e.Property(a => a.InspectionIntervalMonths).HasDefaultValue(Asset.DefaultInspectionIntervalMonths);
            e.HasIndex(a => a.NextInspectionDate);

            e.HasOne(a => a.Owner).WithMany().HasForeignKey(a => a.OwnerId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(a => a.Category).WithMany(c => c.Assets).HasForeignKey(a => a.CategoryId).OnDelete(DeleteBehavior.Restrict);

            e.OwnsOne(a => a.Address, address =>
            {
                address.ToTable("AssetAddresses");
                address.WithOwner().HasForeignKey("AssetId");
                ConfigureAddress(address);
            });
        });

        b.Entity<AppUser>().OwnsOne(u => u.Address, address =>
        {
            address.ToTable("UserAddresses");
            address.WithOwner().HasForeignKey("UserId");
            ConfigureAddress(address);
        });

        b.Entity<AssetCategory>(e =>
        {
            e.Property(c => c.Barcode).HasMaxLength(13);
            e.HasIndex(c => c.Barcode).IsUnique();
            e.Property(c => c.Name).HasMaxLength(100);
            e.HasIndex(c => c.Name).IsUnique();
            e.Property(c => c.Description).HasMaxLength(255);

            // Ids match the old AssetType enum (+1) so the migration can map existing devices.
            e.HasData(
                new AssetCategory { Id = 1, Name = "Laptop" },
                new AssetCategory { Id = 2, Name = "Monitor" },
                new AssetCategory { Id = 3, Name = "Drukarka" },
                new AssetCategory { Id = 4, Name = "Telefon" },
                new AssetCategory { Id = 5, Name = "Inne" });
        });

        b.Entity<Inspection>(e =>
        {
            e.HasOne(i => i.Asset).WithOne(a => a.Inspection).HasForeignKey<Inspection>(i => i.AssetId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(i => i.Owner).WithMany().HasForeignKey(i => i.OwnerId).OnDelete(DeleteBehavior.SetNull);
            e.OwnsOne(i => i.Address, address =>
            {
                address.ToTable("InspectionAddresses");
                address.WithOwner().HasForeignKey("InspectionId");
                ConfigureAddress(address);
            });
        });

        b.Entity<InspectionRecord>(e =>
        {
            e.Property(r => r.Cost).HasPrecision(12, 2);
            e.Property(r => r.Description).HasMaxLength(2000);
            e.HasIndex(r => r.Status);
            e.HasIndex(r => r.PerformedAt);
            e.HasOne(r => r.Inspection).WithMany(i => i.Records).HasForeignKey(r => r.InspectionId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(r => r.CreatedBy).WithMany().HasForeignKey(r => r.CreatedById).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.ReviewedBy).WithMany().HasForeignKey(r => r.ReviewedById).OnDelete(DeleteBehavior.SetNull);
        });

        b.Entity<AssetPhoto>(e =>
        {
            e.Property(x => x.StoredName).HasMaxLength(60);
            e.Property(x => x.OriginalName).HasMaxLength(255);
            e.Property(x => x.ContentType).HasMaxLength(50);
            e.HasOne(x => x.Asset).WithMany(a => a.Photos).HasForeignKey(x => x.AssetId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.UploadedBy).WithMany().HasForeignKey(x => x.UploadedById).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<AssetHistory>(e =>
        {
            e.ToTable("AssetHistory");
            e.Property(h => h.Field).HasMaxLength(50);
            e.Property(h => h.OldValue).HasColumnType("text");
            e.Property(h => h.NewValue).HasColumnType("text");
            e.HasIndex(h => h.ChangedAt);
            e.HasOne(h => h.Asset).WithMany(a => a.History).HasForeignKey(h => h.AssetId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(h => h.ChangedBy).WithMany().HasForeignKey(h => h.ChangedById).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<Comment>(e =>
        {
            e.Property(c => c.Content).HasColumnType("longtext");
            e.HasOne(c => c.Ticket).WithMany(t => t.Comments).HasForeignKey(c => c.TicketId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(c => c.Author).WithMany().HasForeignKey(c => c.AuthorId).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<TicketHistory>(e =>
        {
            e.ToTable("TicketHistory");
            e.Property(h => h.Field).HasMaxLength(50);
            e.Property(h => h.OldValue).HasColumnType("text");
            e.Property(h => h.NewValue).HasColumnType("text");
            e.HasOne(h => h.Ticket).WithMany(t => t.History).HasForeignKey(h => h.TicketId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(h => h.ChangedBy).WithMany().HasForeignKey(h => h.ChangedById).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<RefreshToken>(e =>
        {
            e.Property(r => r.TokenHash).HasMaxLength(64);
            e.HasIndex(r => r.TokenHash).IsUnique();
            e.Property(r => r.ReplacedByHash).HasMaxLength(64);
            e.HasOne<AppUser>().WithMany().HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureAddress<TOwner>(OwnedNavigationBuilder<TOwner, Address> address)
        where TOwner : class
    {
        address.Property(a => a.Street).HasMaxLength(100);
        address.Property(a => a.BuildingNumber).HasMaxLength(20);
        address.Property(a => a.ApartmentNumber).HasMaxLength(20);
        address.Property(a => a.PostalCode).HasMaxLength(12);
        address.Property(a => a.City).HasMaxLength(100);
        address.Property(a => a.Country).HasMaxLength(100);
    }

    private sealed class DateOnlyConverter() : ValueConverter<DateOnly, DateTime>(
        d => d.ToDateTime(TimeOnly.MinValue),
        d => DateOnly.FromDateTime(d));
}
