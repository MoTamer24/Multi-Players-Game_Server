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

        public async override Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier ?? Context.ConnectionId!;

            // 1. Get the match this USER (not connectionId) was in
            var matchId = _manager.GetMatchIdForConnection(userId);

            if (matchId != null)
            {
                var game = _manager.GetGameState(matchId);
                if (game != null)
                {
                    await Clients.Group(matchId).SendAsync("OpponentDisconnected", userId);
                    await Clients.Group(matchId).SendAsync("ReceiveMsg", "Opponent left. You win!");

                    // 3. Cleanup the game
                    _manager.RemoveGame(matchId);
                }
            }

            _manager.RemoveConnection(userId);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task MakeMove(object movedata, string matchId)
        {
            var playerId = Context.UserIdentifier ?? Context.ConnectionId;

            var gameState = _manager.GetGameState(matchId);

            if (gameState is null)
            {
                await Clients.Caller.SendAsync("ReceiveMsg", "Server Error: Game not found");
                return;
            }

            var moveResult = gameState.MakeMove(playerId, movedata);

            // Always send the board state (both success and failure cases)
            var targetClients = moveResult.Success ? Clients.Group(matchId) : Clients.Caller;
            await targetClients.SendAsync("BoardUpdate", gameState.GetBoardState(), gameState.CurrentTurnPlayerId);

            if (moveResult.Success)
            {
                // Check if game ended after the move
                if (gameState.IsGameOver)
                {
                    await Clients.Group(matchId).SendAsync("GameOver", gameState.WinnerId);
                    _manager.RemoveGame(matchId);
                }
            }
            else
            {
                // Send error message only on invalid moves
                await Clients.Caller.SendAsync("ReceiveMsg", moveResult.ErrorMessage);
            }
        }

        public async Task FindMatch(GameType type)
        {
            // Use ConnectionId as the unique identifier
            var playerId = Context.UserIdentifier ?? Context.ConnectionId;

            // FindMatch now returns the userId of a waiting player
            var opponentId = _manager.FindMatch(type, playerId);

            if (opponentId is null)
            {
                await Clients.Caller.SendAsync("ReceiveMsg", "You are on waiting list...");
            }
            else
            {
                var matchId = Guid.NewGuid().ToString();

                // Both players join the same SignalR group using Connection IDs
                await Groups.AddToGroupAsync(Context.ConnectionId, matchId);
                if (_manager.TryGetConnectionId(opponentId, out var oppConn))
                {
                    await Groups.AddToGroupAsync(oppConn, matchId);
                }

                _manager.CreateGame(matchId, type, playerId, opponentId);
                var game = _manager.GetGameState(matchId)!;

                await Clients.Group(matchId).SendAsync("GameStarted", matchId, game.GetBoardState(), game.CurrentTurnPlayerId);
                await Clients.Group(matchId).SendAsync("ReceiveMsg", "Match Found! Game Started.");
            }
        }
    }
}