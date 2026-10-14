using System.Text.Json;
using GameServer.Domain.Entities;

namespace GameServer.Domain.Games
{
    public class TicTacToeGame : IGame
    {
        private readonly char[] _board = new char[9];
        private static readonly int[][] WinningLines = new int[][]
        {
            new[] {0, 1, 2}, new[] {3, 4, 5}, new[] {6, 7, 8}, // Rows
            new[] {0, 3, 6}, new[] {1, 4, 7}, new[] {2, 5, 8}, // Cols
            new[] {0, 4, 8}, new[] {2, 4, 6}                   // Diagonals
        };

        public string CurrentTurnPlayerId { get; private set; }
        public bool IsGameOver { get; set; }
        public string? WinnerId { get; set; }
        public string GameId { get; }
        public string PlayerX_Id { get; private set; }
        public string PlayerO_Id { get; private set; }

        public TicTacToeGame(string gameId, string playerXId, string playerOId)
        {
            GameId = gameId;
            PlayerX_Id = playerXId;
            PlayerO_Id = playerOId;
            CurrentTurnPlayerId = playerXId; // X always starts
            Array.Fill(_board, '-');
        }

        public void TransferOwnership(string oldUserId, string newUserId)
        {
            if (PlayerX_Id == oldUserId) PlayerX_Id = newUserId;
            if (PlayerO_Id == oldUserId) PlayerO_Id = newUserId;
            if (CurrentTurnPlayerId == oldUserId) CurrentTurnPlayerId = newUserId;
            if (WinnerId == oldUserId) WinnerId = newUserId;
        }

        public MoveResult MakeMove(string playerId, object moveData)
        {
            if (IsGameOver) return new MoveResult(false, "Game over");
            if (playerId != CurrentTurnPlayerId) return new MoveResult(false, "not your turn");

            int index;
            if (moveData is JsonElement jsonElement)
            {
                if (!jsonElement.TryGetProperty("cellIndex", out var indexProp))
                {
                    return new MoveResult(false, "Invalid move format.");
                }
                index = indexProp.GetInt32();
            }
            else if (moveData is int directIndex)
            {
                index = directIndex;
            }
            else
            {
                return new MoveResult(false, "Unrecognized move data.");
            }

            if (index < 0 || index > 8) return new MoveResult(false, "invalid move");
            if (_board[index] != '-') return new MoveResult(false, "Cell taken");

            _board[index] = (playerId == PlayerX_Id) ? 'X' : 'O';

            CheckWinner();

            if (!IsGameOver)
            {
                CurrentTurnPlayerId = (CurrentTurnPlayerId == PlayerX_Id) ? PlayerO_Id : PlayerX_Id;
            }

            return new MoveResult(true);
        }

        private void CheckWinner()
        {
            foreach (var line in WinningLines)
            {
                if (_board[line[0]] != '-' &&
                    _board[line[0]] == _board[line[1]] &&
                    _board[line[1]] == _board[line[2]])
                {
                    IsGameOver = true;
                    char winnerSymbol = _board[line[0]];
                    WinnerId = (winnerSymbol == 'X') ? PlayerX_Id : PlayerO_Id;
                    return;
                }
            }

            if (!_board.Contains('-'))
            {
                IsGameOver = true;
                WinnerId = "DRAW";
            }
        }

        public object GetBoardState() => (char[])_board.Clone();
    }
}
