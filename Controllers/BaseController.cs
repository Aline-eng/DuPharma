using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using DuPharma.Data;
using System.Security.Claims;

namespace DuPharma.Controllers;

public class BaseController : Controller
{
    protected readonly AppDbContext _context;

    public BaseController(AppDbContext context)
    {
        _context = context;
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Set pending contact messages count for all pages
        if (User.Identity?.IsAuthenticated == true)
        {
            ViewBag.PendingContactMessages = await _context.ContactMessages.CountAsync(cm => !cm.IsReplied);
        }

        await base.OnActionExecutionAsync(context, next);
    }

    protected int? GetCurrentUserBranchId()
    {
        if (!User.Identity?.IsAuthenticated ?? true)
            return null;

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out var userId))
        {
            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
            return user?.BranchId;
        }

        return null;
    }
}
