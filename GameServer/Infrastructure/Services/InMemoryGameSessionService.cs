using System.Collections.Concurrent;
using GameServer.Application.Interfaces;
using GameServer.Domain.Entities;
using GameServer.Domain.Enums;
using GameServer.Domain.Factories;

namespace GameServer.Infrastructure.Services
{
    public class InMemoryGameSessionService : IGameSessionService
    {
        private readonly ConcurrentDictionary<string, IGame> _games = new();
        private readonly ConcurrentDictionary<string, string> _playerMatches = new();
        private readonly IGameFactory _gameFactory;

        public InMemoryGameSessionService(IGameFactory gameFactory)
        {
            _gameFactory = gameFactory;
        }

        public void CreateGame(string matchId, GameType type, string player1Id, string player2Id)
        {
            var game = _gameFactory.CreateGame(matchId, type, player1Id, player2Id);
            _games.AddOrUpdate(matchId, game, (_, existing) => existing);

            _playerMatches.AddOrUpdate(player1Id, matchId, (_, _) => matchId);
            _playerMatches.AddOrUpdate(player2Id, matchId, (_, _) => matchId);
        }

        public IGame? GetGame(string matchId)
        {
            _games.TryGetValue(matchId, out var game);
            return game;
        }

        public string? GetMatchIdForPlayer(string playerId)
        {
            _playerMatches.TryGetValue(playerId, out var matchId);
            return matchId;
        }

        public bool IsPlayerInGame(string playerId)
        {
            return _playerMatches.ContainsKey(playerId);
        }

        public void RemoveGame(string matchId)
        {
            if (_games.TryRemove(matchId, out _))
            {
                var playersToRemove = _playerMatches
                    .Where(kv => kv.Value == matchId)
                    .Select(kv => kv.Key)
                    .ToList();

                foreach (var playerId in playersToRemove)
                {
                    _playerMatches.TryRemove(playerId, out _);
                }
            }
        }
    }
}
