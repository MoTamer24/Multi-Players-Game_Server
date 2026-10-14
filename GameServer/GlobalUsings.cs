// Re-export in global namespace for backward compatibility with existing legacy code/tests
global using GameType = GameServer.Domain.Enums.GameType;
global using IGame = GameServer.Domain.Entities.IGame;
global using MoveResult = GameServer.Domain.Entities.MoveResult;
global using TicTacToeGame = GameServer.Domain.Games.TicTacToeGame;
global using Chess = GameServer.Domain.Games.ChessGame;
