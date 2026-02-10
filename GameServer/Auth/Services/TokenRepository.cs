
using GameServer.Auth;
using Microsoft.EntityFrameworkCore;
public interface ITokenRepository
{
    Task<RefreshToken?> GetRefreshToken(string username, string token);
    Task SaveRefreshToken(Guid username, string token);
    Task RevokeRefreshToken(RefreshToken token);
}

public class TokenRepository : ITokenRepository
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _cfg;

    public TokenRepository(AppDbContext context,IConfiguration IC)
    {
        _context = context;
        _cfg=IC;

    }


    public async Task<RefreshToken?> GetRefreshToken(string userId, string token)
    {
        return await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.UserId.ToString() ==userId  && t.Token == token);
    }

    public async Task SaveRefreshToken(Guid userId, string token)
    {
        var days = _cfg.GetValue<int>("Jwt:RefreshTokenExpirationDays", 7);
        var refreshToken = new RefreshToken
        {
            UserId = userId,
            Token = token,
            ExpiryDate = DateTime.UtcNow.AddDays(days), 
            IsRevoked = false
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();
    }

    public async Task RevokeRefreshToken(RefreshToken token)
    {
        token.IsRevoked = true;
        _context.RefreshTokens.Update(token);
        await _context.SaveChangesAsync();
    }
}