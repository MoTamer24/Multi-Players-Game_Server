using Microsoft.AspNetCore.SignalR;
namespace GameServer.Hubs
{
    class GameHub : Hub
    {
        GameManager _manager;
        public GameHub(GameManager manager)
        {
            _manager = manager;
        }

        public override Task OnDisconnectedAsync(Exception? exception)
            {
                 _manager.DisconnectPlayer(Context.ConnectionId);
                return base.OnDisconnectedAsync(exception);
            }
    
        public async Task Ping()
        {
            Console.WriteLine("there is something");
            var connId = Context.ConnectionId;
            // sendAsync(a signal for the cleint , mesage or arguments)
            await Clients.All.SendAsync("Ping", $"Player {Context.ConnectionId} connected");
        }
      
      

        public async Task MakeMove(int Index, string matchId)
        {
           var playerId = Context.ConnectionId;
           var gameState=_manager.GetGameState(matchId);

           if (gameState is null )
           {
           await Clients.Caller.SendAsync("ReceiveMsg","Server Error");
           return; 
           }

            // check if players is still active online
            var p1= _manager.IsActive(gameState.PlayerO_Id);
            var p2= _manager.IsActive(gameState.PlayerX_Id);

            if (!p1 || !p2)
            {
                await Clients.Caller.SendAsync("ReceiveMsg","you opponent has left");
                await Clients.Group(matchId).SendAsync("GameOver",playerId);
            }

           bool flag =gameState.MakeMove(Index-1,playerId);
           if (flag)
            {
                string nextTurn = (gameState.CurrentTurnPlayerId == gameState.PlayerX_Id) ? "X" : "O";
                await Clients.Group(matchId).SendAsync("BoardUpdate", gameState.Board, nextTurn);
                if (gameState.IsGameOver)
                {
                    await Clients.Group(matchId).SendAsync("GameOver",gameState.WinnerId);
                    _manager.DisconnectPlayer(gameState.PlayerO_Id);
                    _manager.DisconnectPlayer(gameState.PlayerX_Id);
                }
            }
            else{
            string nextTurn = (gameState.CurrentTurnPlayerId == gameState.PlayerX_Id) ? "X" : "O";
            await Clients.Group(matchId).SendAsync("BoardUpdate", gameState.Board, nextTurn);
            await Clients.Caller.SendAsync("ReceiveMsg","you made something wrong , try again ");
            
            }
        }

        public async Task FindMatch()
        {
            var playerId = Context.ConnectionId;
            var opponentId = _manager.FindMatch(playerId);

            if (opponentId is null)
            {
                // caller ?? i think this means return to the user 
                await Clients.Caller.SendAsync("ReceiveMsg", "You are on waiting list");
            }
            else
            {
      

                // creating groups 
                var matchId = Guid.NewGuid().ToString();
                await Groups.AddToGroupAsync(playerId, matchId);
                await Groups.AddToGroupAsync(opponentId, matchId);
                // Creating Game state 
                _manager.CreateGame(matchId,playerId,opponentId);


                var game = _manager.GetGameState(matchId);
                // telling the players
              
                // Tell Player 1 they are X
            
                await Clients.Client(playerId).SendAsync("GameStarted", matchId, game.Board, "X");
                await Clients.Client(opponentId).SendAsync("GameStarted", matchId, game.Board, "O");
                await Clients.Group(matchId).SendAsync("ReceiveMsg", "opponenet found , match started");
               
            }
        }
    }

}