using SQLite;

namespace GlowBook.Mobile.Models.Offline;

public class LocalStaff
{
    [PrimaryKey]
    public int ServerId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? RoleName { get; set; }
    public string? Email { get; set; }
}
