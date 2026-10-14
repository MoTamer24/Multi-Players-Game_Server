using System.Security.Claims;
using GameServer.Application.DTOs;
using GameServer.Application.Interfaces;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace GameServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly IUserProfileService _userProfileService;
        private readonly ITokenRepository _tokenRepository;
        private readonly IConfiguration _cfg;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            ITokenService tokenService,
            IUserProfileService userProfileService,
            ITokenRepository tokenRepository,
            IConfiguration cfg,
            ILogger<AuthController> logger)
        {
            _tokenService = tokenService;
            _userProfileService = userProfileService;
            _tokenRepository = tokenRepository;
            _cfg = cfg;
            _logger = logger;
        }

        [HttpPost("guest")]
        public async Task<IActionResult> Guest([FromBody] GuestLoginRequest guest)
        {
            var displayName = guest.GetEffectiveName();
            var profile = await _userProfileService.CreateGuestProfileAsync(displayName);
            var accessToken = _tokenService.IssueTokenFromUserProfile(profile);

            return Ok(new
            {
                AccessToken = accessToken,
                token = accessToken, // Backwards compatibility for older demo clients
                userId = profile.Id
            });
        }

        [HttpPost("google")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest googleRequest)
        {
            var rawToken = googleRequest.GetToken();
            if (string.IsNullOrWhiteSpace(rawToken))
            {
                return BadRequest("Missing Google token");
            }

            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _cfg["Google:ClientId"] }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(rawToken, settings);

                var profile = await _userProfileService.FindOrCreateProfileAsync(
                    provider: "google",
                    providerId: payload.Subject,
                    email: payload.Email,
                    name: payload.Name
                );

                var newAccessToken = _tokenService.IssueTokenFromUserProfile(profile);
                var newRefreshToken = _tokenService.GenerateRefreshToken();
                await _tokenRepository.SaveRefreshTokenAsync(profile.Id, newRefreshToken);

                return Ok(new
                {
                    AccessToken = newAccessToken,
                    token = newAccessToken,
                    RefreshToken = newRefreshToken,
                    userId = profile.Id
                });
            }
            catch (InvalidJwtException ex)
            {
                _logger.LogWarning(ex, "Invalid Google token received");
                return Unauthorized("Invalid Google Token");
            }
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            if (string.IsNullOrEmpty(request.AccessToken) || string.IsNullOrEmpty(request.RefreshToken))
            {
                return BadRequest("Access token and refresh token are required");
            }

            ClaimsPrincipal principal;
            try
            {
                principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogWarning(ex, "Token validation failed during refresh");
                return Unauthorized("Invalid token");
            }

            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? principal.FindFirst("uid")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var savedRefreshToken = await _tokenRepository.GetRefreshTokenAsync(userId, request.RefreshToken);
            if (savedRefreshToken == null || savedRefreshToken.IsRevoked || savedRefreshToken.ExpiryDate <= DateTime.UtcNow)
            {
                return Unauthorized("Invalid refresh token");
            }

            var newAccessToken = _tokenService.IssueTokenFromClaims(principal.Claims);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            await _tokenRepository.RevokeRefreshTokenAsync(savedRefreshToken);
            if (Guid.TryParse(userId, out var guid))
            {
                await _tokenRepository.SaveRefreshTokenAsync(guid, newRefreshToken);
            }

            return Ok(new
            {
                AccessToken = newAccessToken,
                token = newAccessToken,
                RefreshToken = newRefreshToken
            });
        }

        [HttpPost("revoke")]
        [Authorize]
        public async Task<IActionResult> Revoke([FromBody] string refreshToken)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? User.FindFirst("uid")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var token = await _tokenRepository.GetRefreshTokenAsync(userId, refreshToken);
            if (token == null)
            {
                return BadRequest("Invalid token");
            }

            await _tokenRepository.RevokeRefreshTokenAsync(token);
            return Ok("Token revoked successfully");
        }
    }
}
