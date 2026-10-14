using GameServer.Domain.Enums;

namespace GameServer.Domain.Entities
{
    public interface IGame
    {
        string GameId { get; }
        string CurrentTurnPlayerId { get; }
        bool IsGameOver { get; set; }
        string? WinnerId { get; set; }

        // Transfer any internal references from oldUserId to newUserId (for guest -> user linking)
        void TransferOwnership(string oldUserId, string newUserId);

        // Returns the board state as a generic object (XO sends char[], Chess sends generic FEN string or 2D array)
        object GetBoardState();

        // Core game logic. Takes a move and the player attempting it.
        MoveResult MakeMove(string playerId, object moveData);
    }
}
