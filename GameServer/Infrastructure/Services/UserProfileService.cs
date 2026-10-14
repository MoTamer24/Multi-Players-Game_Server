using GameServer.Application.Interfaces;
using GameServer.Auth;
using GameServer.Auth.Models;
using Microsoft.EntityFrameworkCore;

namespace GameServer.Infrastructure.Services
{
    public class UserProfileService : IUserProfileService
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _cfg;

        public UserProfileService(AppDbContext db, IConfiguration cfg)
        {
            _db = db;
            _cfg = cfg;
        }

        public async Task<UserProfile> CreateGuestProfileAsync(string? displayName = null)
        {
            var now = DateTimeOffset.UtcNow;
            var lifetimeStr = _cfg["Guest:DefaultLifetimeMinutes"] ?? "60";
            var minutes = int.TryParse(lifetimeStr, out var m) ? m : 60;

            var guest = new UserProfile
            {
                Id = Guid.NewGuid(),
                Provider = "guest",
                ProviderId = Guid.NewGuid().ToString(),
                DisplayName = !string.IsNullOrWhiteSpace(displayName) ? displayName : $"Guest-{now.ToUnixTimeSeconds()}",
                IsGuest = true,
                CreatedAt = now,
                GuestExpiresAt = now.Add(TimeSpan.FromMinutes(minutes))
            };

            await _db.UserProfiles.AddAsync(guest);
            await _db.SaveChangesAsync();
            return guest;
        }

        public async Task<UserProfile?> GetByIdAsync(Guid id)
        {
            return await _db.UserProfiles.FindAsync(id);
        }

        public async Task<UserProfile> FindOrCreateProfileAsync(string provider, string providerId, string email, string name)
        {
            var existing = await _db.UserProfiles
                .FirstOrDefaultAsync(u => u.Provider == provider && u.ProviderId == providerId);

            if (existing != null)
            {
                existing.LastSeenAt = DateTimeOffset.UtcNow;
                await _db.SaveChangesAsync();
                return existing;
            }

            var newUser = new UserProfile
            {
                Id = Guid.NewGuid(),
                Provider = provider,
                ProviderId = providerId,
                Email = email,
                DisplayName = name,
                IsGuest = false,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _db.UserProfiles.Add(newUser);
            await _db.SaveChangesAsync();
            return newUser;
        }
    }
}
