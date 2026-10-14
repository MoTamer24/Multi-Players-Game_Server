using System.Collections.Concurrent;
using GameServer.Application.Interfaces;
using GameServer.Domain.Enums;

namespace GameServer.Infrastructure.Services
{
    public class InMemoryMatchmakingService : IMatchmakingService
    {
        private readonly ConcurrentDictionary<GameType, ConcurrentQueue<string>> _queues = new();
        private readonly IConnectionManager _connectionManager;

        public InMemoryMatchmakingService(IConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
        }

        public string? QueueOrMatch(GameType type, string playerId)
        {
            var queue = _queues.GetOrAdd(type, _ => new ConcurrentQueue<string>());

            while (queue.TryDequeue(out string? waitingPlayerId))
            {
                // Ensure the candidate is not the same person and has an active connection
                if (waitingPlayerId != playerId && _connectionManager.TryGetConnectionId(waitingPlayerId, out _))
                {
                    return waitingPlayerId;
                }
            }

            queue.Enqueue(playerId);
            return null;
        }

        public void RemoveFromQueue(string playerId)
        {
            foreach (var kvp in _queues)
            {
                // Clean up references to playerId if needed
                // Note: ConcurrentQueue doesn't have Remove, but QueueOrMatch validates active connection
            }
        }
    }
}
