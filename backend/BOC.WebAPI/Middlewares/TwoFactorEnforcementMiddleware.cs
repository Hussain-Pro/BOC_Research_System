using System;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using BOC.Application.Common.Interfaces;

namespace BOC.WebAPI.Middlewares;

public class TwoFactorEnforcementMiddleware
{
    private readonly RequestDelegate _next;

    public TwoFactorEnforcementMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IBOCDbContext dbContext)
    {
        var path = context.Request.Path.Value ?? "";

        // Skip auth endpoints
        if (path.StartsWith("/api/auth/", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
        {
            var user = await dbContext.AppUsers
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user != null)
            {
                var isAdminRole = user.Role.Name switch
                {
                    "Admin" => true,
                    "Committee Chairman" => true,
                    "Deputy Chairman" => true,
                    "Committee Secretary" => true,
                    _ => false
                };

                if (isAdminRole && !user.TwoFactorEnabled)
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    context.Response.ContentType = "application/json";

                    var problemDetails = new
                    {
                        title = "Two-Factor Authentication Required",
                        status = 403,
                        detail = "You must set up and verify two-factor authentication before you can access this resource."
                    };

                    await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
                    return;
                }
            }
        }

        await _next(context);
    }
}
