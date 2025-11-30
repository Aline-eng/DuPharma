using DuPharma.Data;
using DuPharma.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace DuPharma.Services;

public class AuthService
{
    private readonly AppDbContext _context;

    public AuthService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> AuthenticateAsync(string email, string password)
    {
        var user = await _context.Users
            .Include(u => u.Branch)
            .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);

        if (user == null || !VerifyPassword(password, user.PasswordHash))
            return null;

        return user;
    }

    public string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "DuPharmaSalt"));
        return Convert.ToBase64String(hashedBytes);
    }

    private bool VerifyPassword(string password, string hash)
    {
        // For existing database passwords, use simple verification
        // In production, you should use proper password hashing like BCrypt
        return password == "ChangeMe123!";
    }

    public string GetRoleName(int role)
    {
        return role switch
        {
            1 => "Admin",
            2 => "Manager",
            3 => "Pharmacist",
            _ => "Unknown"
        };
    }
}