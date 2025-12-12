using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DuPharma.Models;

public class ContactMessage
{
    public int Id { get; set; }
    
    [Required, MaxLength(100)]
    public string FullName { get; set; } = string.Empty;
    
    [Required, EmailAddress, MaxLength(100)]
    public string Email { get; set; } = string.Empty;
    
    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;
    
    [Required, MaxLength(200)]
    public string Subject { get; set; } = string.Empty;
    
    [Required, MaxLength(2000)]
    public string Message { get; set; } = string.Empty;

    public bool IsRead { get; set; } = false;

    [MaxLength(2000)]
    public string? Reply { get; set; }

    public bool IsReplied { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime? RepliedAt { get; set; }

    public int? RepliedByUserId { get; set; }

    public User? RepliedByUser { get; set; }

    public int? BranchId { get; set; }

    public Branch? Branch { get; set; }
}

public class ContactFormViewModel
{
    [Required(ErrorMessage = "Full name is required")]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;
    
    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Subject is required")]
    [MaxLength(200)]
    public string Subject { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Message is required")]
    [MaxLength(2000)]
    public string Message { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select a branch")]
    public int BranchId { get; set; }

    // Anti-bot field (should remain empty) - no validation attributes
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string HoneyPot { get; set; } = string.Empty;
}

public class ContactReplyViewModel
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    
    [Required(ErrorMessage = "Reply is required")]
    [MaxLength(2000)]
    public string Reply { get; set; } = string.Empty;
}