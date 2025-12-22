using System.Collections.Concurrent;
namespace GameServer
{
    class GameManager
    {
        // thread safe Queue , what does this means ??
        ConcurrentQueue<string> players;
        Dictionary<string,GameState> GamesList;

        public GameManager()
        {
            players=new ConcurrentQueue<string>();
            GamesList=new ();
        }
        public void CreateGame(string matchId,string playerId,string opponentId)
        {
            GamesList.Add(matchId,new GameState(playerId,opponentId));
        }

        public GameState? GetGameState(string matchId)
        {
            GamesList.TryGetValue(matchId,out var GS); // hmmm, why null ??? it is Try !
            return GS;
        }

        public string? FindMatch(string playerId)
        {
            if (players.TryDequeue(out string? waitingPlayer))
            {
                return waitingPlayer;
            }
            players.Enqueue(playerId);
            return null;
        }    
    }
}