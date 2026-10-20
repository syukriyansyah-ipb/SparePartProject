using Microsoft.EntityFrameworkCore;
using AutoPartsApi.Models;

namespace AutoPartsApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<VehicleModel> VehicleModels => Set<VehicleModel>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Dealer> Dealers => Set<Dealer>();

    public DbSet<Part> Parts => Set<Part>();
    public DbSet<PartCategory> PartCategories => Set<PartCategory>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Konfigurasi Vehicle
        modelBuilder.Entity<Vehicle>()
            .Property(v => v.Price)
            .HasPrecision(18, 2);

        // Konfigurasi Part
        modelBuilder.Entity<Part>(entity =>
        {
            entity.Property(p => p.Price).HasPrecision(18, 2);
            entity.HasIndex(p => p.PartNumber).IsUnique(); // Kode part tidak boleh kembar

            // Relasi ke Kategori
            entity.HasOne(p => p.PartCategory)
                .WithMany(c => c.Parts)
                .HasForeignKey(p => p.PartCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relasi Many-to-Many ke Model Mobil
            entity.HasMany(p => p.CompatibleVehicles)
                .WithMany(v => v.CompatibleParts)
                .UsingEntity(j => j.ToTable("PartCompatibleVehicles"));
        });       
        
    }
}