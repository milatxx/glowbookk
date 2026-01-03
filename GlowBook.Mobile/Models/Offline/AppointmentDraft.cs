namespace GlowBook.Mobile.Models.Offline;

public class AppointmentDraft
{
    public string LocalUid { get; set; } = Guid.NewGuid().ToString("N");
    public int CustomerId { get; set; }
    public int StaffId { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public string Status { get; set; } = "Ingepland";
    public int[] ServiceIds { get; set; } = Array.Empty<int>();
}
