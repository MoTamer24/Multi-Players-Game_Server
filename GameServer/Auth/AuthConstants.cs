
using Microsoft.IdentityModel.Tokens;
using System.Text;
public static class AuthConstants
{

    public  static TokenValidationParameters GetAuthParameters(IConfiguration cfg)
    {

        var key = cfg["Jwt:SigningKey"];
            if (string.IsNullOrEmpty(key))
                throw new InvalidOperationException("APP_JWT_SIGNING_KEY is not configured");

            var _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var _issuer = cfg["Jwt:Issuer"] ?? "GameServer";
            var _audience = cfg["Jwt:Audience"] ?? "gameserver";

         var tvp = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _signingKey,
                ValidateLifetime = true
            };
        return tvp;
    }
}