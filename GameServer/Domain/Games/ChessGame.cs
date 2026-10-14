using GameServer.Domain.Entities;

namespace GameServer.Domain.Games
{
    public class ChessGame : IGame
    {
        public string GameId { get; }
        public string CurrentTurnPlayerId { get; private set; }
        public bool IsGameOver { get; set; }
        public string? WinnerId { get; set; }
        private char[][] _board { get; set; }

        public ChessGame(string gameId, string player1Id, string player2Id)
        {
            GameId = gameId;
            CurrentTurnPlayerId = player1Id;
            _board = Array.Empty<char[]>();
        }

        public object GetBoardState() => _board;

        public MoveResult MakeMove(string playerId, object moveData)
        {
            return new MoveResult(false, "Chess is not implemented yet.");
        }

        public void TransferOwnership(string oldUserId, string newUserId)
        {
            if (CurrentTurnPlayerId == oldUserId) CurrentTurnPlayerId = newUserId;
            if (WinnerId == oldUserId) WinnerId = newUserId;
        }
    }
}
