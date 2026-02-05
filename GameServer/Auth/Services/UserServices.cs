using Microsoft.EntityFrameworkCore;
using GameServer.Auth.Models;
namespace GameServer.Auth.Services
{
     public interface IUserProfileService
    {
        Task<UserProfile> CreateGuestProfile(string? displayName = null);
        Task<UserProfile> FindOrCreateProfile(string provider,string providerId, string email, string name);
        UserProfile? GetById(Guid id);
    }

    public class UserProfileService : IUserProfileService
    {
        readonly AppDbContext _db;
        readonly IConfiguration _cfg;
        public UserProfileService(AppDbContext db,IConfiguration cfg)
        {
            _db = db;
            _cfg=cfg;
            }

        public async Task<UserProfile> CreateGuestProfile(string? displayName = null)
        {
            var now = DateTimeOffset.UtcNow;
            var guest = new UserProfile
            {
                Id = Guid.NewGuid(),
                Provider = "guest",
                ProviderId = Guid.NewGuid().ToString(),
                DisplayName = displayName ?? $"Guest-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}",
                IsGuest = true,
                CreatedAt = now,
                GuestExpiresAt = now.Add(TimeSpan.FromMinutes(int.Parse(_cfg["Guest:DefaultLifetimeMinutes"]!)))
            };
           await _db.UserProfiles.AddAsync(guest);
           await _db.SaveChangesAsync();
           return guest;
        }


        public UserProfile? GetById(Guid id) => _db.UserProfiles.Find(id);

        public async Task<UserProfile> FindOrCreateProfile(string provider,string providerId, string email, string name)
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