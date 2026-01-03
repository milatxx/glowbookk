using SQLite;

namespace GlowBook.Mobile.Models.Offline;

public class LocalCustomer
{
    [PrimaryKey]
    public int ServerId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
}
