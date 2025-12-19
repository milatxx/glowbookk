using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;

namespace GlowBook.Web.Middleware;

public class ErrorHandlingMiddleware : IMiddleware
{
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(ILogger<ErrorHandlingMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Onvrwachte fout bij {Method} {Path}",
                context.Request.Method, context.Request.Path);

            // Veilige check op JSON acceptt header 
            var acceptHeader = context.Request.Headers.Accept.ToString();
            var wantsJson =
                context.Request.Path.StartsWithSegments("/api") ||
                acceptHeader.Contains("application/json", StringComparison.OrdinalIgnoreCase);

            if (wantsJson)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = MediaTypeNames.Application.Json;

                var problem = new ProblemDetails
                {
                    Status = 500,
                    Title = "Interne serverfout",
                    Detail = "Er liep iets mis. Probeer opnieuw of contacteer de beheerder."
                };

                await context.Response.WriteAsJsonAsync(problem);
                return;
            }

            context.Response.Redirect("/Home/Error");
        }
    }
}
