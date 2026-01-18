

namespace GameServer
{
    public abstract class GameState
    {
        
        
    }
    public class XOGameState
    {
        // 'X' or 'O' or '-' (empty)
        public char[] Board { get; private set; } 
        public string PlayerX_Id { get; private set; }
        public string PlayerO_Id { get; private set; }
        public string CurrentTurnPlayerId { get; private set; }
        public bool IsGameOver { get; private set; } = false;
        public string? WinnerId { get; private set; } = null; // Null means no winner yet

        public XOGameState(string xPlayerId, string oPlayerId)
        {
            PlayerX_Id = xPlayerId;
            PlayerO_Id = oPlayerId;
            CurrentTurnPlayerId = xPlayerId; // X always starts

            // Initialize empty board with dashes
            Board = new char[9]; 
            Array.Fill(Board, '-');
        }

        public bool MakeMove(int index, string playerId)
        {
            // 1. Validation Checks
            if (IsGameOver) return false;
            if (playerId != CurrentTurnPlayerId) return false; // Not your turn
            if (index < 0 || index > 8) return false; // Out of bounds
            if (Board[index] != '-') return false; // Spot already taken

            // 2. Update Board
            char symbol = (playerId == PlayerX_Id) ? 'X' : 'O';
            Board[index] = symbol;

            // 3. Check for Win/Draw
            CheckWinner();

            // 4. Switch Turn (only if game is not over)
            if (!IsGameOver)
            {
                CurrentTurnPlayerId = (CurrentTurnPlayerId == PlayerX_Id) ? PlayerO_Id : PlayerX_Id;
            }

            return true; // Move successful
        }

        private void CheckWinner()
        {
            // All possible winning combinations (indices)
            int[][] winningLines = new int[][]
            {
                new[] {0, 1, 2}, // Row 1
                new[] {3, 4, 5}, // Row 2
                new[] {6, 7, 8}, // Row 3
                new[] {0, 3, 6}, // Col 1
                new[] {1, 4, 7}, // Col 2
                new[] {2, 5, 8}, // Col 3
                new[] {0, 4, 8}, // Diagonal 1
                new[] {2, 4, 6}  // Diagonal 2
            };

            foreach (var line in winningLines)
            {
                if (Board[line[0]] != '-' && 
                    Board[line[0]] == Board[line[1]] && 
                    Board[line[1]] == Board[line[2]])
                {
                    // We have a winner!
                    IsGameOver = true;
                    char winnerSymbol = Board[line[0]];
                    WinnerId = (winnerSymbol == 'X') ? PlayerX_Id : PlayerO_Id;
                    return;
                }
            }

            // Check for Draw (Board full, no winner)
            if (!Board.Contains('-'))
            {
                IsGameOver = true;
                WinnerId = "DRAW";
            }
        }
    }
}