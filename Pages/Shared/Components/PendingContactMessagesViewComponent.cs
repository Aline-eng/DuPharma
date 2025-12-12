using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DuPharma.Data;
using System.Security.Claims;

namespace DuPharma.ViewComponents;

public class PendingContactMessagesViewComponent : ViewComponent
{
    private readonly AppDbContext _context;

    public PendingContactMessagesViewComponent(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var query = _context.ContactMessages.AsQueryable();

        // Filter by user's branch if not admin
        var userBranchIdClaim = ViewContext.HttpContext.User.FindFirst("BranchId")?.Value;
        if (int.TryParse(userBranchIdClaim, out var userBranchId))
        {
            query = query.Where(cm => cm.BranchId == userBranchId || cm.BranchId == null);
        }

        var count = await query.CountAsync(cm => !cm.IsReplied);
        return View(count);
    }
}
