using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GameServer.Auth.Models;

public class RefreshToken
{
    [Key]
    public int Id { get; set; }
    public Guid UserId { get; set; }  // Foreign Key to AppUser
    public string Token { get; set; } = string.Empty;

    public DateTime ExpiryDate { get; set; }
    public bool IsRevoked { get; set; }

    // Navigation property (optional but helpful for EF)
    [ForeignKey("UserId")]
    public virtual UserProfile User { get; set; }
}