using System;
using Microsoft.EntityFrameworkCore;

namespace GameServer.Auth.Models
{
    //< Tamer > wht is this 
    [Index(nameof(Provider), nameof(ProviderId), IsUnique = true)]
    public class UserProfile
    {
        public Guid Id { get; set; }
        public string Provider { get; set; } = "guest"; // 'google' or 'guest'
        public string ProviderId { get; set; } = string.Empty; // external sub or generated guest id
        public string? DisplayName { get; set; }
        public string? Email { get; set; }
        public bool EmailVerified { get; set; }
        public bool IsGuest { get; set; }
        public DateTimeOffset? GuestExpiresAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? LastSeenAt { get; set; }
        public string? Metadata { get; set; }
    }

    public class UserConnection
    {
        public string ConnectionId { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public UserProfile user{get;set;}=null!;
        public DateTimeOffset ConnectedAt { get; set; }
        public DateTimeOffset? LastSeenAt { get; set; }
    }
}
