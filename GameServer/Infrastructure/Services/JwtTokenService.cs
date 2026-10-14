using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using GameServer.Application.Interfaces;
using GameServer.Auth;
using GameServer.Auth.Models;
using Microsoft.IdentityModel.Tokens;

namespace GameServer.Infrastructure.Services
{
    public class JwtTokenService : ITokenService
    {
        private readonly SymmetricSecurityKey _signingKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly IConfiguration _cfg;

        public JwtTokenService(IConfiguration cfg)
        {
            _cfg = cfg;
            var key = Environment.GetEnvironmentVariable("JWT__SigningKey")
                      ?? cfg["Jwt:SigningKey"]
                      ?? cfg["Jwt:SigningKey"];

            if (string.IsNullOrEmpty(key))
                throw new InvalidOperationException("APP_JWT_SIGNING_KEY / Jwt:SigningKey is not configured");

            _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            _issuer = cfg["Jwt:Issuer"] ?? "GameServer";
            _audience = cfg["Jwt:Audience"] ?? "gameserver";
        }

        public string IssueTokenFromUserProfile(UserProfile profile)
        {
            var uid = profile.Id.ToString();
            var isGuest = profile.IsGuest;
            var name = profile.DisplayName ?? "Unknown";
            var now = DateTimeOffset.UtcNow;

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, uid),
                new Claim("uid", uid), // Included so frontend parseJwt(token).uid works seamlessly!
                new Claim(ClaimTypes.Name, name),
                new Claim("unique_name", name), // Included for frontend parseJwt(token).unique_name
                new Claim("is_guest", isGuest ? "true" : "false")
            };

            string configKey = isGuest ? "Guest:DefaultLifetimeMinutes" : "User:DefaultLifetimeMinutes";
            var minutesStr = _cfg[configKey] ?? (isGuest ? "60" : "1440");
            var expiration = double.TryParse(minutesStr, out var m) ? m : (isGuest ? 60 : 1440);

            var cred = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);
            var jwt = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                notBefore: now.UtcDateTime,
                expires: now.Add(TimeSpan.FromMinutes(expiration)).UtcDateTime,
                signingCredentials: cred);

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        public string IssueTokenFromClaims(IEnumerable<Claim> claims)
        {
            var now = DateTime.UtcNow;
            var creds = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);
            var expires = now.AddMinutes(15);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                notBefore: now,
                expires: expires,
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = AuthConstants.GetAuthParameters(_cfg);
            tokenValidationParameters.ValidateLifetime = false;

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token signature or algorithm");
            }

            return principal;
        }
    }
}
