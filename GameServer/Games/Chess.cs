public class Chess : IGame
{
    public string CurrentTurnPlayerId { get; private set; }
    public bool IsGameOver { get; set; }
    public string? WinnerId { get; set; }

    public string GameId { get; }
    private char[][] _board { get; set; }

    public object GetBoardState() => _board;
    public MoveResult MakeMove(string playerId, object MoveData)
    {
        return new MoveResult(false);
    }

    // Chess is not implemented yet; provide a no-op transfer to satisfy IGame contract
    public void TransferOwnership(string oldUserId, string newUserId) { /* no-op */ }
}