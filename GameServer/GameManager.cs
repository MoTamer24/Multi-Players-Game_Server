using System.Collections.Concurrent;
using System.Text.RegularExpressions;
namespace GameServer
{
    class GameManager
    {
        // thread safe Queue , what does this means ??
        ConcurrentDictionary<GameType,ConcurrentQueue<string>> WaitingPlayers;
        ConcurrentDictionary<string, bool> OnlinePlayers;

        ConcurrentDictionary<string, IGame> GamesList;

        public GameManager()
        {
            WaitingPlayers = new ();
            OnlinePlayers = new ConcurrentDictionary<string, bool>();
            GamesList = new();
        }
        public void DisconnectPlayer(string playerId)
        {
            OnlinePlayers.AddOrUpdate(playerId,false,(key,existingValue)=>existingValue);
        }
        public bool IsActive(string playerId)
        {
            OnlinePlayers.TryGetValue(playerId,out bool res);
            return res ;
        }


        public void CreateGame(string matchId, GameType type,string playerId, string opponentId)
        {
            GamesList.AddOrUpdate(
                           matchId,
                          GameFactory.CreateGame(matchId,type,playerId,opponentId),        // add: create new GameState
                           (key, existingValue) => existingValue     // update: decide what to do if matchId already exists
                                );
        }


        public IGame? GetGameState(string matchId)
        {
            GamesList.TryGetValue(matchId, out var GS); 
            return GS;
        }

        public string? FindMatch(GameType type,string playerId)
        {
             OnlinePlayers.AddOrUpdate(playerId,true,(key,existingValue) => existingValue);
           var queue = WaitingPlayers.GetOrAdd(type, new ConcurrentQueue<string>());

    if (queue.TryDequeue(out string? waitingPlayer))
    {
         if(waitingPlayer != playerId && IsActive(waitingPlayer)) 
             return waitingPlayer;
    }
            // if he didn't find an opponenet , put him on waiting list.
            queue.Enqueue(playerId);
          
            return null;
        }
    }
}