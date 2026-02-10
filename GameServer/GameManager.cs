using System.Collections.Concurrent;
namespace GameServer
{
    public class GameManager
    {
        // Waiting players per game type (keyed by userId)
        ConcurrentDictionary<GameType, ConcurrentQueue<string>> WaitingPlayers;


        //string  : to store connectionId : MatchID 
        // to trach active players 
        ConcurrentDictionary<string, bool> _userConnections;

        // Games keyed by matchId
        ConcurrentDictionary<string, IGame> GamesList;

        // Track which match each player is in (userId -> matchId)
        ConcurrentDictionary<string, string> _playerMatches;

        public GameManager()
        {
            WaitingPlayers = new();
            _userConnections = new();
            GamesList = new();
            _playerMatches = new();
        }

        // Connection management
        public void AddConnection( string connectionId)
        {
            _userConnections.TryAdd(connectionId,true);
        }

        public void RemoveConnection(string connectionId)
        {
            _userConnections.TryRemove(connectionId, out _);
        }


       public string? GetMatchIdForConnection(string connectionId)
    {
        _playerMatches.TryGetValue(connectionId, out var matchId);
        return matchId;
    }

        public void CreateGame(string matchId, GameType type, string playerId, string opponentId)
        {
            GamesList.AddOrUpdate(
                           matchId,
                          GameFactory.CreateGame(matchId, type, playerId, opponentId),
                           (key, existingValue) => existingValue
                                );

            // Track which match each player is in
            
            _playerMatches.TryAdd(playerId, matchId);
            _playerMatches.TryAdd(opponentId, matchId);
        }

        public IGame? GetGameState(string matchId)
        {
            GamesList.TryGetValue(matchId, out var GS);
            return GS;
        }

        public bool IsActive(string userId)
        {
            return _playerMatches.ContainsKey(userId);
        }

        public void RemoveGame(string matchId)
        {
            if (GamesList.TryRemove(matchId, out var game))
            {
                // Remove player match associations
                var playersToRemove = _playerMatches.Where(kv => kv.Value == matchId).Select(kv => kv.Key).ToList();
                foreach (var player in playersToRemove)
                {
                    _playerMatches.TryRemove(player, out _);
                }
            }
        }

     public string? FindMatch(GameType type, string connectionId)
    {
        var queue = WaitingPlayers.GetOrAdd(type, _ => new ConcurrentQueue<string>());

        while (queue.TryDequeue(out string? waitingConnectionId))
        {
            // Ensure the waiting player is still connected and isn't the same person
            if (waitingConnectionId != connectionId && _userConnections.ContainsKey(waitingConnectionId))
                return waitingConnectionId;
        }

        queue.Enqueue(connectionId);
        return null;
    }
    }
}


