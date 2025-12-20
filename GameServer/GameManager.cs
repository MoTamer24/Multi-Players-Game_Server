using System.Collections.Concurrent;
namespace GameServer
{
    class GameManager
    {
        // thread safe Queue , what does this means ??
        ConcurrentQueue<string> players;
        public GameManager()
        {
            players=new ConcurrentQueue<string>();
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