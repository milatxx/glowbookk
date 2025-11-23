using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GlowBook.Web.Controllers.Api;

[Authorize]
public class HomeController : Controller
{
    public IActionResult Index() => View();

    [AllowAnonymous]
    public IActionResult Privacy() => View();

    [AllowAnonymous]
    public IActionResult Error() => View();
}
    