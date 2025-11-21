using Microsoft.AspNetCore.Identity;

namespace GlowBook.Model.Entities;

public class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
