using SQLite;

namespace GlowBook.Mobile.Models.Offline;

public class LocalAppointmentV2
{
    [PrimaryKey]
    public string LocalUid { get; set; } = Guid.NewGuid().ToString("N");

    // 0 = nog niet op server
    public int ServerId { get; set; }

    public DateTime Start { get; set; }
    public DateTime End { get; set; }

    public int CustomerId { get; set; }
    public int StaffId { get; set; }

    // 1 service of meerdere via csv
    public string ServiceIdsCsv { get; set; } = "";

    public string CustomerName { get; set; } = "";
    public string StaffName { get; set; } = "";
    public string ServiceName { get; set; } = "";
    public string Status { get; set; } = "";

    // Offline pending
    public bool IsPending { get; set; }

    public DateTime UpdatedUtc { get; set; } = DateTime.UtcNow;
}
