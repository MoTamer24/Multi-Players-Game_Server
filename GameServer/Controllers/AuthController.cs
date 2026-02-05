
using Microsoft.AspNetCore.Mvc;
using GameServer.Auth.Services;
using Google.Apis.Auth;

[ApiController]
[Route("api/[controller]")]
public class AuthController:ControllerBase
{
    readonly ITokenService tokenService;
    readonly IUserProfileService userProfileService;
    readonly IConfiguration cfg;

   public AuthController(
            ITokenService tokenService, 
            IUserProfileService userProfileService, 
            IConfiguration cfg)
        {
            this.tokenService = tokenService;
            this.userProfileService = userProfileService;
            this.cfg = cfg;
        }
    [HttpPost("guest")]
    public async Task<IActionResult> Guest(GuestDto guest)
    {
        var profile =await userProfileService.CreateGuestProfile(guest.DisplayName);
        var token=tokenService.IssueToken(profile);
        return Ok(new{token});
    }

   [HttpPost("google")]
public async Task<IActionResult> GoogleLogin([FromBody] string googleIdToken)
{
    try
    {
        // 1. Verify the token with Google
        var settings = new GoogleJsonWebSignature.ValidationSettings()
        {
            Audience = new[] { cfg["Google:ClientId"] }
        };

        var payload = await GoogleJsonWebSignature.ValidateAsync(googleIdToken, settings);

        // 2. Sync with your DB
        var profile = await userProfileService.FindOrCreateProfile(
            "google",
            payload.Subject, // This is the unique Google ID
            payload.Email, 
            payload.Name
        );

        // 3. Issue your Game Server Token
        var token = tokenService.IssueToken(profile);

        return Ok(new { token });
    }
    catch (InvalidJwtException)
    {
        return Unauthorized("Invalid Google Token");
    }
}
}