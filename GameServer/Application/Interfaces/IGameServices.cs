using System.Collections.Concurrent;
using GameServer.Domain.Entities;
using GameServer.Domain.Enums;
using GameServer.Domain.Factories;

namespace GameServer.Application.Interfaces
{
    public interface IMatchmakingService
    {
        string? QueueOrMatch(GameType type, string playerId);
        void RemoveFromQueue(string playerId);
    }

    public interface IGameSessionService
    {
        void CreateGame(string matchId, GameType type, string player1Id, string player2Id);
        IGame? GetGame(string matchId);
        string? GetMatchIdForPlayer(string playerId);
        void RemoveGame(string matchId);
        bool IsPlayerInGame(string playerId);
    }

    public interface IConnectionManager
    {
        void AddConnection(string userId, string connectionId);
        void RemoveConnection(string userId);
        bool TryGetConnectionId(string userId, out string connectionId);
        string? GetConnectionId(string userId);
    }
}
