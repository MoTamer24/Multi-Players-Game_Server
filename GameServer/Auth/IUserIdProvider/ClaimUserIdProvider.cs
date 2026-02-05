using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace GameServer.Auth
{
    public class ClaimUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            var principal = connection.User as ClaimsPrincipal;
            // Prefer the application 'uid' claim (local GUID). Fallback to NameIdentifier.
            var uid = principal?.FindFirst("uid")?.Value;
            if (!string.IsNullOrEmpty(uid)) return uid;
            return principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
