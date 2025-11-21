using Microsoft.AspNetCore.Http;
using System.Globalization;
using System.Threading.Tasks;

public class CultureCookieMiddleware : IMiddleware
{
    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var lang = context.Request.Query["lang"].ToString();
        if (!string.IsNullOrWhiteSpace(lang))
        {
            context.Response.Cookies.Append("glowbook_culture", lang, new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true
            });

            var culture = new CultureInfo(lang);
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
        }

        return next(context);
    }
}
