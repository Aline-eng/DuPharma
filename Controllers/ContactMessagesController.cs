using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DuPharma.Data;
using DuPharma.Models;
using DuPharma.Attributes;
using DuPharma.Services;
using System.Security.Claims;

namespace DuPharma.Controllers;

[RequirePermission(Permissions.ViewContactMessages)]
public class ContactMessagesController : Controller
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;

    public ContactMessagesController(AppDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public async Task<IActionResult> Index(string filter = "all")
    {
        var query = _context.ContactMessages
            .Include(cm => cm.RepliedByUser)
            .AsQueryable();

        switch (filter.ToLower())
        {
            case "replied":
                query = query.Where(cm => cm.IsReplied);
                break;
            case "pending":
                query = query.Where(cm => !cm.IsReplied);
                break;
            default:
                // Show all
                break;
        }

        var messages = await query
            .OrderByDescending(cm => cm.CreatedAt)
            .ToListAsync();

        ViewBag.Filter = filter;
        ViewBag.PendingCount = await _context.ContactMessages.CountAsync(cm => !cm.IsReplied);
        ViewBag.RepliedCount = await _context.ContactMessages.CountAsync(cm => cm.IsReplied);
        ViewBag.TotalCount = await _context.ContactMessages.CountAsync();

        return View(messages);
    }

    public async Task<IActionResult> Details(int id)
    {
        var message = await _context.ContactMessages
            .Include(cm => cm.RepliedByUser)
            .FirstOrDefaultAsync(cm => cm.Id == id);

        if (message == null)
            return NotFound();

        return View(message);
    }

    [RequirePermission(Permissions.ReplyContactMessages)]
    public async Task<IActionResult> Reply(int id)
    {
        var message = await _context.ContactMessages.FindAsync(id);
        if (message == null)
            return NotFound();

        var viewModel = new ContactReplyViewModel
        {
            Id = message.Id,
            FullName = message.FullName,
            Email = message.Email,
            Phone = message.Phone,
            Subject = message.Subject,
            Message = message.Message,
            CreatedAt = message.CreatedAt
        };

        return View(viewModel);
    }

    [HttpPost]
    [RequirePermission(Permissions.ReplyContactMessages)]
    public async Task<IActionResult> Reply(ContactReplyViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var message = await _context.ContactMessages.FindAsync(model.Id);
        if (message == null)
            return NotFound();

        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            message.Reply = model.Reply;
            message.IsReplied = true;
            message.RepliedAt = DateTime.Now;
            message.RepliedByUserId = userId;

            await _context.SaveChangesAsync();

            // Send email reply
            await _emailService.SendContactReplyAsync(
                message.Email,
                message.FullName,
                message.Message,
                model.Reply
            );

            TempData["Success"] = "Reply sent successfully!";
            return RedirectToAction(nameof(Details), new { id = model.Id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "An error occurred while sending the reply. Please try again.");
            return View(model);
        }
    }

    [RequirePermission(Permissions.DeleteContactMessages)]
    public async Task<IActionResult> Delete(int id)
    {
        var message = await _context.ContactMessages.FindAsync(id);
        if (message == null)
            return NotFound();

        return View(message);
    }

    [HttpPost, ActionName("Delete")]
    [RequirePermission(Permissions.DeleteContactMessages)]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var message = await _context.ContactMessages.FindAsync(id);
        if (message != null)
        {
            _context.ContactMessages.Remove(message);
            await _context.SaveChangesAsync();
        }

        TempData["Success"] = "Message deleted successfully!";
        return RedirectToAction(nameof(Index));
    }
}