using GlowBook.Model.Entities;
using Microsoft.AspNetCore.Identity;

namespace GlowBook.Web.Middleware;

public class ActiveUserMiddleware : IMiddleware
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public ActiveUserMiddleware(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            var user = await _userManager.GetUserAsync(context.User);

            if (user != null && !user.IsActive)
            {
                await _signInManager.SignOutAsync();
                context.Response.Redirect("/Account/Login?blocked=1");
                return;
            }
        }

        await next(context);
    }
}
