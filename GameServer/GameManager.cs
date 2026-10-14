using GameServer.Application.Interfaces;
using GameServer.Domain.Entities;
using GameServer.Domain.Enums;

namespace GameServer
{
    /// <summary>
    /// Facade providing backward compatibility for any existing callers expecting GameManager.
    /// Internally delegates to modular, testable, and scalable services.
    /// </summary>
    public class GameManager
    {
        private readonly IConnectionManager _connectionManager;
        private readonly IMatchmakingService _matchmakingService;
        private readonly IGameSessionService _gameSessionService;

        public GameManager(
            IConnectionManager connectionManager,
            IMatchmakingService matchmakingService,
            IGameSessionService gameSessionService)
        {
            _connectionManager = connectionManager;
            _matchmakingService = matchmakingService;
            _gameSessionService = gameSessionService;
        }

        public void AddConnection(string userId, string connectionId) =>
            _connectionManager.AddConnection(userId, connectionId);

        public void RemoveConnection(string userId) =>
            _connectionManager.RemoveConnection(userId);

        public bool TryGetConnectionId(string userId, out string connectionId) =>
            _connectionManager.TryGetConnectionId(userId, out connectionId);

        public string? GetMatchIdForConnection(string userIdOrConnectionId) =>
            _gameSessionService.GetMatchIdForPlayer(userIdOrConnectionId);

        public void CreateGame(string matchId, GameType type, string playerId, string opponentId) =>
            _gameSessionService.CreateGame(matchId, type, playerId, opponentId);

        public IGame? GetGameState(string matchId) =>
            _gameSessionService.GetGame(matchId);

        public bool IsActive(string userId) =>
            _gameSessionService.IsPlayerInGame(userId);

        public void RemoveGame(string matchId) =>
            _gameSessionService.RemoveGame(matchId);

        public string? FindMatch(GameType type, string playerId) =>
            _matchmakingService.QueueOrMatch(type, playerId);
    }
}
