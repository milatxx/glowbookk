using SQLite;

namespace GlowBook.Mobile.Models.Offline;

public class LocalService
{
    [PrimaryKey]
    public int ServerId { get; set; }

    public string Name { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
}
