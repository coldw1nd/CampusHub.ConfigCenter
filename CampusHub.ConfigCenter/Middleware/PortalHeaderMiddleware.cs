using CampusHub.ConfigCenter.Models;
using Microsoft.Extensions.Options;

namespace CampusHub.ConfigCenter.Middleware;

public class PortalHeaderMiddleware
{
    private readonly RequestDelegate _next;
    private readonly PortalOptions _options;

    public PortalHeaderMiddleware(RequestDelegate next, IOptions<PortalOptions> options)
    {
        _next = next;
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.Headers["X-Portal-Title"] = _options.Title;
        context.Response.Headers["X-Portal-Semester"] = _options.Semester;
        
        await _next(context);
    }
}