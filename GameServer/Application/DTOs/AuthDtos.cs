using GameServer.Domain.Enums;

namespace GameServer.Application.DTOs
{
    public class GuestLoginRequest
    {
        public string? DisplayName { get; set; }
        public string? GuestName { get; set; }

        public string GetEffectiveName() =>
            !string.IsNullOrWhiteSpace(DisplayName) ? DisplayName : (!string.IsNullOrWhiteSpace(GuestName) ? GuestName : "Guest");
    }

    public class GoogleLoginRequest
    {
        public string? Idptoken { get; set; }
        public string? Credential { get; set; }

        public string? GetToken() =>
            !string.IsNullOrWhiteSpace(Idptoken) ? Idptoken : Credential;
    }

    public class RefreshTokenRequest
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class AuthResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string? RefreshToken { get; set; }
        public Guid UserId { get; set; }
        public string Token => AccessToken;
    }
}
