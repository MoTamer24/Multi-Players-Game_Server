// The contract that ALL games must follow
public interface IGame
{
    string GameId { get; }
    string CurrentTurnPlayerId { get; }
    bool IsGameOver { get; }
    string? WinnerId { get; }
    
    // Returns the board state as a generic object (XO sends char[], Chess sends generic FEN string or 2D array)
    object GetBoardState();
    
    // The core logic. Takes a generic move and the player trying to make it.
    MoveResult MakeMove(string playerId, object moveData);  // object ?
}

// A helper result to tell the Hub what happened
public record MoveResult(bool Success, string ErrorMessage = null);

public enum GameType
{
    TicTacToe, Chess
}