using System.Collections.Concurrent;
namespace GameServer
{
    public class GameManager
    {
        // Waiting players per game type (keyed by userId)
        ConcurrentDictionary<GameType, ConcurrentQueue<string>> WaitingPlayers;

        // Map userId -> set of connectionIds
        // if a player have many cleints
        ConcurrentDictionary<string, ConcurrentDictionary<string, bool>> _userConnections;

        // Games keyed by matchId
        ConcurrentDictionary<string, IGame> GamesList;

        public GameManager()
        {
            WaitingPlayers = new();
            _userConnections = new();
            GamesList = new();
        }

        // Connection management
        public void AddConnection(string userId, string connectionId)
        {
            var conns = _userConnections.GetOrAdd(userId, _ => new ConcurrentDictionary<string, bool>());
            conns.TryAdd(connectionId, true);
        }

        public void RemoveConnection(string connectionId)
        {
            // find and remove the connection from any user mapping
            foreach (var kv in _userConnections)
            {
                if (kv.Value.TryRemove(connectionId, out _))
                {
                    // if no more connections, mark user as offline and remove waiting list entries
                    if (kv.Value.IsEmpty)
                    {
                        _userConnections.TryRemove(kv.Key, out _);
                        // remove from waiting lists
                        foreach (var q in WaitingPlayers.Values)
                        {
                            // best-effort: rebuild queue without this user (cheap for small queues)
                            var items = q.ToArray();
                            var newQ = new ConcurrentQueue<string>(items.Where(id => id != kv.Key));
                            while (q.TryDequeue(out _)) { }
                            foreach (var it in newQ) q.Enqueue(it);
                        }
                    }
                    break;
                }
            }
        }

        public IEnumerable<string> GetConnections(string userId)
        {
            if (_userConnections.TryGetValue(userId, out var conns))
                return conns.Keys;
            return Array.Empty<string>();
        }

        // Transfer active games/waitlists from oldUserId to newUserId (guest -> permanent link)
        public void ReassignPlayerId(string oldUserId, string newUserId)
        {
            // move connections
            if (_userConnections.TryRemove(oldUserId, out var conns))
            {
                var target = _userConnections.GetOrAdd(newUserId, _ => new ConcurrentDictionary<string, bool>());
                foreach (var c in conns.Keys) target.TryAdd(c, true);
            }

            // update waiting lists
            foreach (var kv in WaitingPlayers)
            {
                var q = kv.Value;
                var items = q.ToArray();
                var newQ = new ConcurrentQueue<string>(items.Select(id => id == oldUserId ? newUserId : id));
                while (q.TryDequeue(out _)) { }
                foreach (var it in newQ) q.Enqueue(it);
            }

            // update games
            foreach (var kv in GamesList)
            {
                kv.Value.TransferOwnership(oldUserId, newUserId);
            }
        }

        public void CreateGame(string matchId, GameType type, string playerId, string opponentId)
        {
            GamesList.AddOrUpdate(
                           matchId,
                          GameFactory.CreateGame(matchId, type, playerId, opponentId),
                           (key, existingValue) => existingValue
                                );
        }

        public IGame? GetGameState(string matchId)
        {
            GamesList.TryGetValue(matchId, out var GS);
            return GS;
        }

        public bool IsActive(string userId)
        {
            return _userConnections.TryGetValue(userId, out var v) && !v.IsEmpty;
        }

        public string? FindMatch(GameType type, string userId)
        {
            // ensure user is active
            if (!IsActive(userId)) AddConnection(userId, userId);

            var queue = WaitingPlayers.GetOrAdd(type, _ => new ConcurrentQueue<string>());

            if (queue.TryDequeue(out string? waitingPlayer))
            {
                if (waitingPlayer != userId && IsActive(waitingPlayer))
                    return waitingPlayer;
            }

            queue.Enqueue(userId);
            return null;
        }
    }
}

 
     