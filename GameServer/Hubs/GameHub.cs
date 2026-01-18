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

        public async Task MakeMove(object movedata, string matchId)
        {
            var playerId = Context.ConnectionId;
            var gameState = _manager.GetGameState(matchId);

            if (gameState is null)
            {
                await Clients.Caller.SendAsync("ReceiveMsg", "Server Error: Game not found");
                return;
            }

            var moveResult = gameState.MakeMove(playerId, movedata);
            
            if (moveResult.Success)
            {
                // Send the updated board to everyone
                await Clients.Group(matchId).SendAsync("BoardUpdate", gameState.GetBoardState(), gameState.CurrentTurnPlayerId);

                if (gameState.IsGameOver)
                {
                    await Clients.Group(matchId).SendAsync("GameOver", gameState.WinnerId);
                }
            }
            else
            {
                await Clients.Caller.SendAsync("BoardUpdate", gameState.GetBoardState(), gameState.CurrentTurnPlayerId);
                await Clients.Caller.SendAsync("ReceiveMsg", moveResult.ErrorMessage);
            }
        }

        public async Task FindMatch(GameType type)
        {
            var playerId = Context.ConnectionId;
            var opponentId = _manager.FindMatch(type, playerId);

            if (opponentId is null)
            {
                await Clients.Caller.SendAsync("ReceiveMsg", "You are on waiting list...");
            }
            else
            {
                var matchId = Guid.NewGuid().ToString();
                
                await Groups.AddToGroupAsync(playerId, matchId);
                await Groups.AddToGroupAsync(opponentId, matchId);
                
                _manager.CreateGame(matchId, type, playerId, opponentId);
                var game = _manager.GetGameState(matchId);

                await Clients.Group(matchId).SendAsync("GameStarted", matchId, game.GetBoardState(), game.CurrentTurnPlayerId);
                
                await Clients.Group(matchId).SendAsync("ReceiveMsg", "Match Found! Game Started.");
            }
        }
    }
}