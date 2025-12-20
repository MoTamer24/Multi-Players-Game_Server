using Microsoft.AspNetCore.SignalR;
namespace GameServer.Hubs
{
    class GameHub : Hub
    {
        GameManager _manager;
        public GameHub(GameManager manager )
        {
            _manager= manager; 
        }
    public async Task Ping()
    {
        Console.WriteLine("there is something");
        var connId=Context.ConnectionId;
        // sendAsync(a signal for the cleint , mesage or arguments)
        await Clients.All.SendAsync("Ping", $"Player {Context.ConnectionId} connected");
    }
      public async Task MakeMove(string move,string matchId)
        {
            await Clients.Group(matchId).SendAsync("ReceiveMsg",move);
        }
    public async Task FindMatch()
        {
            var playerId=Context.ConnectionId;
            var opponentId=_manager.FindMatch(playerId);
            if (opponentId is null)
            {
                // caller ???  i think this means return to the user 
              await Clients.Caller.SendAsync("ReceiveMsg", "You are on waiting list");
            }
            else{
            var matchId=Guid.NewGuid().ToString();
            await Groups.AddToGroupAsync(playerId,matchId);
            await Groups.AddToGroupAsync(opponentId,matchId);

            await Clients.Group(matchId).SendAsync("GameStarted", matchId);
            await Clients.Group(matchId).SendAsync("ReceiveMsg", "Match Found! Game Starting...");
            }
        }
    }
    
}