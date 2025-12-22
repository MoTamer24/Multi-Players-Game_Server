using System.Collections.Concurrent;
using System.Text.RegularExpressions;
namespace GameServer
{
    class GameManager
    {
        // thread safe Queue , what does this means ??
        ConcurrentQueue<string> WaitingPlayers;
        ConcurrentDictionary<string, bool> OnlinePlayers;

        ConcurrentDictionary<string, GameState> GamesList;

        public GameManager()
        {
            WaitingPlayers = new ConcurrentQueue<string>();
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


        public void CreateGame(string matchId, string playerId, string opponentId)
        {
            GamesList.AddOrUpdate(
                           matchId,
                           new GameState(playerId, opponentId),        // add: create new GameState
                           (key, existingValue) => existingValue     // update: decide what to do if matchId already exists
                                );
        }


        public GameState? GetGameState(string matchId)
        {
            GamesList.TryGetValue(matchId, out var GS); 
            return GS;
        }

        public string? FindMatch(string playerId)
        {
             OnlinePlayers.AddOrUpdate(playerId,true,(key,existingValue) => existingValue);
            if (WaitingPlayers.TryDequeue(out string? waitingPlayer))
            {
                // checking if opponent is still online
                if(waitingPlayer !=playerId && OnlinePlayers[waitingPlayer])
               
                return waitingPlayer;
            }
            // if he didn't find an opponenet , put him on waiting list.
            WaitingPlayers.Enqueue(playerId);
          
            return null;
        }
    }
}