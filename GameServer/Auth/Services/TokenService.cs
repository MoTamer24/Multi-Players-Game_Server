using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using GameServer.Auth.Models;

namespace GameServer.Auth.Services
{
    public interface ITokenService
    {
        string IssueToken(UserProfile profile);
        ClaimsPrincipal? ValidateAppToken(string token);
    }

    // Minimal IUserProfileService (small in-file implementation for MVP)
    public interface IUserProfileService
    {
        UserProfile CreateGuestProfile(string? displayName = null, TimeSpan? lifetime = null);
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

        public UserProfile CreateGuestProfile(string? displayName = null, TimeSpan? lifetime = null)
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
                GuestExpiresAt = now.Add(lifetime ?? TimeSpan.FromMinutes(int.Parse(_cfg["Guest:DefaultLifetimeMinutes"]!)))
            };
            _db.UserProfiles.Add(guest);
            _db.SaveChanges();
            return guest;
        }

        public UserProfile? GetById(Guid id) => _db.UserProfiles.Find(id);
    }

    public class TokenService : ITokenService
    {
        readonly SymmetricSecurityKey _signingKey;
        readonly string _issuer;
        readonly string _audience;
        readonly IConfiguration _cfg;
        public TokenService(IConfiguration cfg)
        {
            _cfg=cfg;
            var key = Environment.GetEnvironmentVariable("APP_JWT_SIGNING_KEY")
                      ?? cfg["AppJwt:SigningKey"]
                      ?? cfg["Jwt:SigningKey"];
            if (string.IsNullOrEmpty(key))
                throw new InvalidOperationException("APP_JWT_SIGNING_KEY is not configured");

            _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            _issuer = cfg["Jwt:Issuer"] ?? "GameServer";
            _audience = cfg["Jwt:Audience"] ?? "gameserver";
        }

        public string IssueToken(UserProfile profile)
        {

            var uid=profile.Id.ToString();
            var isGuest=profile.IsGuest;
            var name=profile.DisplayName??"Unknown";
            var now = DateTimeOffset.UtcNow;

            var claims = new[] {
                new Claim("uid", uid),
                new Claim(ClaimTypes.Name, name),
                new Claim("is_guest", isGuest ? "true" : "false")
            };

            string configKey = isGuest ? "Guest:DefaultLifetimeMinutes" : "User:DefaultLifetimeMinutes";
            var minutesStr = _cfg[configKey] ?? (isGuest ? "60" : "1440"); // user token lasts for an entir day until we made refresh token

            var cred = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);
            var jwt = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                notBefore: now.UtcDateTime,
                expires: now.Add(TimeSpan.Parse(minutesStr)).UtcDateTime,
                signingCredentials: cred);

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        public ClaimsPrincipal? ValidateAppToken(string token)
        {
            var tvp = AuthConstants.GetAuthParameters(_cfg);
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var principal = handler.ValidateToken(token, tvp, out _);
                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}
