using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using DuPharma.Data;
using DuPharma.Models;
using DuPharma.Services;

namespace DuPharma.Controllers;

[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly AppDbContext _context;
    private readonly AuthService _authService;

    public UsersController(AppDbContext context, AuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _context.Users
            .Include(u => u.Branch)
            .OrderBy(u => u.FullName)
            .ToListAsync();
        return View(users);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Branches = await _context.Branches.ToListAsync();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserViewModel model)
    {
        ModelState.Remove("Branch");
        
        if (ModelState.IsValid)
        {
            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email already exists");
                ViewBag.Branches = await _context.Branches.ToListAsync();
                return View(model);
            }

            var user = new User
            {
                FullName = model.FullName,
                Email = model.Email,
                PasswordHash = _authService.HashPassword(model.Password),
                Phone = model.Phone,
                BranchId = model.BranchId,
                Role = model.Role,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Branches = await _context.Branches.ToListAsync();
        return View(model);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        var model = new EditUserViewModel
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.Phone,
            BranchId = user.BranchId,
            Role = user.Role,
            IsActive = user.IsActive
        };

        ViewBag.Branches = await _context.Branches.ToListAsync();
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, EditUserViewModel model)
    {
        if (id != model.UserId) return NotFound();

        ModelState.Remove("Branch");

        if (ModelState.IsValid)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            if (await _context.Users.AnyAsync(u => u.Email == model.Email && u.UserId != id))
            {
                ModelState.AddModelError("Email", "Email already exists");
                ViewBag.Branches = await _context.Branches.ToListAsync();
                return View(model);
            }

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.Phone = model.Phone;
            user.BranchId = model.BranchId;
            user.Role = model.Role;
            user.IsActive = model.IsActive;

            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                user.PasswordHash = _authService.HashPassword(model.NewPassword);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Branches = await _context.Branches.ToListAsync();
        return View(model);
    }
}

public class CreateUserViewModel
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public int? BranchId { get; set; }
    public int Role { get; set; } = 3;
}

public class EditUserViewModel
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? NewPassword { get; set; }
    public string Phone { get; set; } = string.Empty;
    public int? BranchId { get; set; }
    public int Role { get; set; }
    public bool IsActive { get; set; }
}
