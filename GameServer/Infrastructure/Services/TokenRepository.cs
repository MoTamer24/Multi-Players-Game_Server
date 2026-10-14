using GameServer.Application.Interfaces;
using GameServer.Auth;
using GameServer.Auth.Models;
using Microsoft.EntityFrameworkCore;

namespace GameServer.Infrastructure.Services
{
    public class TokenRepository : ITokenRepository
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _cfg;

        public TokenRepository(AppDbContext context, IConfiguration cfg)
        {
            _context = context;
            _cfg = cfg;
        }

        public async Task<RefreshToken?> GetRefreshTokenAsync(string userId, string token)
        {
            if (!Guid.TryParse(userId, out var parsedId)) return null;

            return await _context.RefreshTokens
                .FirstOrDefaultAsync(t => t.UserId == parsedId && t.Token == token);
        }

        public async Task SaveRefreshTokenAsync(Guid userId, string token)
        {
            var days = _cfg.GetValue<int>("Jwt:RefreshTokenExpirationDays", 7);
            var refreshToken = new RefreshToken
            {
                UserId = userId,
                Token = token,
                ExpiryDate = DateTime.UtcNow.AddDays(days),
                IsRevoked = false
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();
        }

        public async Task RevokeRefreshTokenAsync(RefreshToken token)
        {
            token.IsRevoked = true;
            _context.RefreshTokens.Update(token);
            await _context.SaveChangesAsync();
        }
    }
}
