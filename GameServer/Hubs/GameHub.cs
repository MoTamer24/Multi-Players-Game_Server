using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace GameServer.Hubs
{
    [Authorize]
    public class GameHub : Hub
    {
        GameManager _manager;
        public GameHub(GameManager manager)
        {
            _manager = manager;
        }

        public override Task OnConnectedAsync()
        {
            // Map authenticated user (uid claim) to this connection; fall back to ConnectionId during migration
            var userId = Context.UserIdentifier ?? Context.ConnectionId;
            _manager.AddConnection(userId, Context.ConnectionId);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            // Remove connection mapping and possibly mark user offline
            _manager.RemoveConnection(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }

        public async Task MakeMove(object movedata, string matchId)
        {
            var playerId = Context.UserIdentifier ?? Context.ConnectionId;

        //<Tamer> why is this line if i do get the connections id ? i think this need edit 
            if (Context.UserIdentifier is null)
            {
                await Clients.Caller.SendAsync("ReceiveMsg", "Unauthorized: missing user identity");
                return;
            }

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
                //<Tamer : can't i get those repetitive lines of updating the baord before the if condition
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
            if (Context.UserIdentifier is null)
            {
                await Clients.Caller.SendAsync("ReceiveMsg", "Unauthorized: missing user identity");
                return;
            }

            var playerId = Context.UserIdentifier;
            var opponentId = _manager.FindMatch(type, playerId);

            if (opponentId is null)
            {
                await Clients.Caller.SendAsync("ReceiveMsg", "You are on waiting list...");
            }
            else
            {
                var matchId = Guid.NewGuid().ToString();

                // Add all active connections for each user to the SignalR group
                foreach (var conn in _manager.GetConnections(playerId))
                    await Groups.AddToGroupAsync(conn, matchId);
                foreach (var conn in _manager.GetConnections(opponentId))
                    await Groups.AddToGroupAsync(conn, matchId);

                _manager.CreateGame(matchId, type, playerId, opponentId);
                var game = _manager.GetGameState(matchId)!;
                System.Console.WriteLine(game.CurrentTurnPlayerId);
                await Clients.Group(matchId).SendAsync("GameStarted", matchId, game.GetBoardState(), game.CurrentTurnPlayerId);

                await Clients.Group(matchId).SendAsync("ReceiveMsg", "Match Found! Game Started.");
            }
        }
    }
}