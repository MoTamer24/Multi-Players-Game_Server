namespace GameServer.Domain.Entities
{
    public record MoveResult(bool Success, string? ErrorMessage = null);
}
