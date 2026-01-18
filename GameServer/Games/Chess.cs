public class Chess : IGame
{
    public string CurrentTurnPlayerId { get; private set; }
    public bool IsGameOver { get; private set; }
    public string? WinnerId { get; private set; }
    public string GameId { get; }
    private char[] [] _board{get;set;}

    public object GetBoardState() => _board;
    public MoveResult MakeMove(string playerId,object MoveData)
    {
        return new MoveResult(false);
    }

}