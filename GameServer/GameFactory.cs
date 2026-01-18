public static class GameFactory{
    public static IGame CreateGame(string GameId,GameType type, string p1, string p2)
    {
        switch (type)
        {
            case GameType.TicTacToe :
            return new TicTacToeGame(GameId,p1,p2);
            case GameType.Chess:
                // Return new ChessGame(matchId, p1Id, p2Id); 
                throw new NotImplementedException("Chess is coming soon!");
            default:
                throw new ArgumentException("Unknown game type");
        }
    }
}