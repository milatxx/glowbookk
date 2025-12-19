namespace GlowBook.Web.Models.Admin;

public class EditUserRolesViewModel
{
    public string UserId { get; set; } = "";
    public string Email { get; set; } = "";
    public string DisplayName { get; set; } = "";

    public List<RoleCheckItem> Roles { get; set; } = [];


    public class RoleCheckItem
    {
        public string RoleName { get; set; } = "";
        public bool IsSelected { get; set; }
    }
}
