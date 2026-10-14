using GameServer.Domain.Entities;
using GameServer.Domain.Enums;
using GameServer.Domain.Games;

namespace GameServer.Domain.Factories
{
    public interface IGameFactory
    {
        IGame CreateGame(string gameId, GameType type, string player1Id, string player2Id);
    }

    public class GameFactory : IGameFactory
    {
        public IGame CreateGame(string gameId, GameType type, string player1Id, string player2Id)
        {
            return type switch
            {
                GameType.TicTacToe => new TicTacToeGame(gameId, player1Id, player2Id),
                GameType.Chess => throw new NotImplementedException("Chess is coming soon!"),
                _ => throw new ArgumentException($"Unknown game type: {type}", nameof(type))
            };
        }
    }
}
