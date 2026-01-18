using Microsoft.AspNetCore.SignalR;

public class TicTacToeGame : IGame
{
    private char[] _board = new char[9];
    public string CurrentTurnPlayerId { get; private set; }
    public bool IsGameOver { get; private set; }
    public string? WinnerId { get; private set; }
    public string GameId { get; }
    public string PlayerX_Id{get;}
    public string PlayerO_Id{get;}

    public TicTacToeGame(string Gameid, string xId, string oId)
    {
        GameId = Gameid;
        PlayerX_Id = xId;
        PlayerO_Id = oId;
        CurrentTurnPlayerId = xId; // X starts
        Array.Fill(_board, '-');
    }

    public MoveResult MakeMove(string playerId, object moveData)
    {
        if (IsGameOver) return new MoveResult(false,"Game over");
        if (playerId != CurrentTurnPlayerId) return new MoveResult(false,"not your turn"); 

         // 2. Deserialize the specific move for THIS game type
        // The Hub sends us a JSON element (System.Text.Json)
        int index;
        try {
            index = Convert.ToInt32(moveData.ToString());
        } catch {
            return new MoveResult(false, "Invalid move format for XO");
        }

        // 1. Validation Checks
        if (index < 0 || index > 8) return new MoveResult(false,"invalid move"); 
        if (_board[index] != '-') return new MoveResult(false,"Cell taken"); 
        
       if (IsGameOver) return new MoveResult(false, "Game over");
       if (playerId != CurrentTurnPlayerId) return new MoveResult(false, "Not your turn");
        _board[index] = (playerId == PlayerX_Id) ? 'X' : 'O';
        

    // --- CHECK WINNER ---
    CheckWinner();

    // --- SWITCH TURN (Only if game is NOT over) ---
    if (!IsGameOver)
    {
        CurrentTurnPlayerId = (CurrentTurnPlayerId == PlayerX_Id) ? PlayerO_Id : PlayerX_Id;
    }
    return new MoveResult(true);

    }
private void CheckWinner()
{
    // All possible winning combinations (indices)
    int[][] winningLines = new int[][]
    {
        new[] {0, 1, 2}, new[] {3, 4, 5}, new[] {6, 7, 8}, // Rows
        new[] {0, 3, 6}, new[] {1, 4, 7}, new[] {2, 5, 8}, // Cols
        new[] {0, 4, 8}, new[] {2, 4, 6}                   // Diagonals
    };

    foreach (var line in winningLines)
    {
        // Check if the first cell is not empty AND all three match
        if (_board[line[0]] != '-' && 
            _board[line[0]] == _board[line[1]] && 
            _board[line[1]] == _board[line[2]])
        {
            IsGameOver = true;
            char winnerSymbol = _board[line[0]];
            WinnerId = (winnerSymbol == 'X') ? PlayerX_Id : PlayerO_Id;
            return; // Exit immediately, we found a winner
        }
    }

    // Check for Draw (Board full, no winner)
    if (!_board.Contains('-'))
    {
        IsGameOver = true;
        WinnerId = "DRAW";
    }
}
    public object GetBoardState() => _board;
}