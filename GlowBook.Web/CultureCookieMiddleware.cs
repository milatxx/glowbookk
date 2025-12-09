using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;

namespace GlowBook.Web;

public class CultureCookieMiddleware : IMiddleware
{
    private const string CookieName = "glowbook_culture";

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var lang = context.Request.Query["lang"].ToString();

        if (!string.IsNullOrWhiteSpace(lang))
        {
            try
            {
                var culture = new CultureInfo(lang);
                var requestCulture = new RequestCulture(culture, culture);

                context.Response.Cookies.Append(
                    CookieName,
                    CookieRequestCultureProvider.MakeCookieValue(requestCulture),
                    new CookieOptions
                    {
                        Expires = DateTimeOffset.UtcNow.AddYears(1),
                        IsEssential = true,
                        HttpOnly = false
                    });

                CultureInfo.CurrentCulture = culture;
                CultureInfo.CurrentUICulture = culture;

                context.Features.Set<IRequestCultureFeature>(
                    new RequestCultureFeature(
                        requestCulture,
                        provider: new CookieRequestCultureProvider()));
            }
            catch (CultureNotFoundException)
            {
            }
        }

        await next(context);
    }
}
