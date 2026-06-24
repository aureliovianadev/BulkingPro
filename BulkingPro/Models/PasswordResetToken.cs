// Models/PasswordResetToken.cs
#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BulkingPro.Models;

public class PasswordResetToken
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public string UserId { get; set; } = null!;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
    
    [Required]
    public string Code { get; set; } = null!;
    
    public DateTime ExpiresAt { get; set; }
    
    public bool Used { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    [ForeignKey("UserId")]
    public virtual Usuario User { get; set; } = null!;
}