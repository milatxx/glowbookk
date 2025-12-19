namespace GlowBook.Web.Services;

public class EmailSettings
{
    public string FromName { get; set; } = "GlowBook";
    public string FromEmail { get; set; } = "";
    public string Host { get; set; } = "";
    public int Port { get; set; } = 587;
    public bool UseStartTls { get; set; } = true;
    public string User { get; set; } = "";
    public string Password { get; set; } = ""; // komt uit secrets
}
