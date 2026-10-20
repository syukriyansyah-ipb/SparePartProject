namespace AutoPartsApi.Models;

public class Part
{
    public int Id { get; set; }
    public string PartNumber { get; set; } = string.Empty; // Kode unik (misal: MD123456)
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    // Harga & Stok
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public int MinimumStock { get; set; } = 5; // Untuk alert jika stok menipis
    public string? RackLocation { get; set; } // Lokasi rak gudang (misal: A-01)
    
    // Relasi ke Kategori
    public int PartCategoryId { get; set; }
    public PartCategory? PartCategory { get; set; }

    // Relasi ke Model Mobil (Misal: Filter ini cocok untuk Xpander)
    // Untuk mempermudah, kita buat Many-to-Many dengan VehicleModel
    public ICollection<VehicleModel> CompatibleVehicles { get; set; } = new List<VehicleModel>();
    
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}