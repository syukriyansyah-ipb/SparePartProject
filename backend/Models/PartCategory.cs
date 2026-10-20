namespace AutoPartsApi.Models;

public class PartCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty; // "Oli", "Sistem Rem"
    public string? Description { get; set; }
    
    // Navigation property
    public ICollection<Part> Parts { get; set; } = new List<Part>();
}