namespace GameServer.Auth.Models
{
    public class MatchRecord
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Player1Id { get; set; } = string.Empty;
        public string Player2Id { get; set; } = string.Empty;
        public string? WinnerId { get; set; } // Null if it's a draw
        public GameType GameType { get; set; }
        public DateTime EndedAt { get; set; } = DateTime.UtcNow;
    }
}