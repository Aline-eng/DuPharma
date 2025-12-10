using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using DuPharma.Services;
using System.Security.Claims;

namespace DuPharma.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class RequirePermissionAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly string _permission;

    public RequirePermissionAttribute(string permission)
    {
        _permission = permission;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        // Check if user is authenticated
        if (!context.HttpContext.User.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new RedirectToActionResult("Login", "Account", null);
            return;
        }

        // Get user ID from claims
        var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            context.Result = new ForbidResult();
            return;
        }

        // Get permission service
        var permissionService = context.HttpContext.RequestServices.GetService<IPermissionService>();
        if (permissionService == null)
        {
            context.Result = new StatusCodeResult(500);
            return;
        }

        // Check if user has the required permission
        var hasPermission = await permissionService.HasPermissionAsync(userId, _permission);
        if (!hasPermission)
        {
            context.Result = new ForbidResult();
            return;
        }
    }
}

// Extension method for easier permission checking in controllers
public static class PermissionExtensions
{
    public static async Task<bool> HasPermissionAsync(this ClaimsPrincipal user, IPermissionService permissionService, string permission)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            return false;

        return await permissionService.HasPermissionAsync(userId, permission);
    }
}