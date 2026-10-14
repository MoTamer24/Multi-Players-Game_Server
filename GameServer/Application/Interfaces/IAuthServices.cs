using System.Security.Claims;
using GameServer.Auth.Models;

namespace GameServer.Application.Interfaces
{
    public interface ITokenService
    {
        string IssueTokenFromUserProfile(UserProfile profile);
        string IssueTokenFromClaims(IEnumerable<Claim> claims);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }

    public interface IUserProfileService
    {
        Task<UserProfile> CreateGuestProfileAsync(string? displayName = null);
        Task<UserProfile> FindOrCreateProfileAsync(string provider, string providerId, string email, string name);
        Task<UserProfile?> GetByIdAsync(Guid id);
    }

    public interface ITokenRepository
    {
        Task<RefreshToken?> GetRefreshTokenAsync(string userId, string token);
        Task SaveRefreshTokenAsync(Guid userId, string token);
        Task RevokeRefreshTokenAsync(RefreshToken token);
    }
}
