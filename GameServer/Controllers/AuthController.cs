

using Microsoft.AspNetCore.Mvc;
using GameServer.Auth.Services;

public class AuthController:ControllerBase
{
    readonly ITokenService tokenService;
    public AuthController()
    {
        
    }
    [HttpPost]
    public Task<string> Guest(GuestDto guest)
    {
        var profile =  
        tokenService.IssueToken();
    }

    [HttpPost]
    public Task<string> Exchange(idPToken token)
    {
        
    }
}