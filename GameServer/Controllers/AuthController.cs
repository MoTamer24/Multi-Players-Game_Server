
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GameServer.Auth.Services;
using Google.Apis.Auth;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    readonly ITokenService _tokenService;
    readonly IUserProfileService _userProfileService;
    readonly IConfiguration _cfg;
    readonly ITokenRepository _tokenRepository;

    public AuthController(
             ITokenService tokenService,
             IUserProfileService userProfileService,
             IConfiguration cfg,
             ITokenRepository TR)
    {
        this._tokenService = tokenService;
        this._userProfileService = userProfileService;
        this._cfg = cfg;
        _tokenRepository = TR;
    }
    [HttpPost("guest")]
    public async Task<IActionResult> Guest(GuestDto guest)
    {
        var profile = await _userProfileService.CreateGuestProfile(guest.DisplayName);
        var userId=profile.Id;
        var AccessToken = _tokenService.IssueTokenFromUserProfile(profile);
        return Ok(new { AccessToken , userId});
    }

    [HttpPost("google")]
    public async Task<IActionResult> GoogleLogin([FromBody] string googleIdToken)
    {
        try
        {
            // 1. Verify the token with Google
            var settings = new GoogleJsonWebSignature.ValidationSettings()
            {
                Audience = new[] { _cfg["Google:ClientId"] }
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(googleIdToken, settings);

            // 2. Sync with your DB
            var profile = await _userProfileService.FindOrCreateProfile(
                "google",
                payload.Subject, // This is the unique Google ID
                payload.Email,
                payload.Name
            );
        var newAccessToken = _tokenService.IssueTokenFromUserProfile(profile);
        var newRefreshToken = _tokenService.GenerateRefreshToken();
        await _tokenRepository.SaveRefreshToken(profile.Id, newRefreshToken);
        var userId= profile.Id;
        return Ok(new
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            userId
        });
        }
        catch (InvalidJwtException)
        {
            return Unauthorized("Invalid Google Token");
        }
    }


    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] TokenRequest request)
    {
        var principal = GetPrincipalFromExpiredToken(request.AccessToken);
        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var savedRefreshToken = await _tokenRepository.GetRefreshToken(userId, request.RefreshToken);

        if (savedRefreshToken == null || savedRefreshToken.IsRevoked || savedRefreshToken.ExpiryDate <= DateTime.UtcNow)
        {
            return Unauthorized("Invalid refresh token");
        }

        var newAccessToken = _tokenService.IssueTokenFromClaims(principal.Claims);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

       
        await _tokenRepository.RevokeRefreshToken(savedRefreshToken);
        await _tokenRepository.SaveRefreshToken(Guid.Parse(userId), newRefreshToken);

        return Ok(new
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken
        });
    }
    private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = AuthConstants.GetAuthParameters(_cfg);

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

        if (!(securityToken is JwtSecurityToken jwtSecurityToken) ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            throw new SecurityTokenException("Invalid token");

        return principal;
    }
    [HttpPost("revoke")]
    [Authorize] // This one SHOULD be authorized because the user is currently logged in
    public async Task<IActionResult> Revoke([FromBody] string refreshToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        var token = await _tokenRepository.GetRefreshToken(userId, refreshToken);

        if (token == null) return BadRequest("Invalid token");

        await _tokenRepository.RevokeRefreshToken(token);
        return Ok("Token revoked successfully");
    }
}

