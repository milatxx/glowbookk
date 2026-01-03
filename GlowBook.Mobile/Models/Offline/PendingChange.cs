using SQLite;

namespace GlowBook.Mobile.Models.Offline;

public class PendingChange
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Operation { get; set; } = "";

    // Json
    public string PayloadJson { get; set; } = "";

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}
