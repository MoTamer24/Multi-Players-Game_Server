using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using GameServer.Application.Interfaces;
using GameServer.Domain.Entities;
using GameServer.Domain.Enums;

namespace GameServer.Hubs
{
    [Authorize]
    public class GameHub : Hub
    {
        private readonly IConnectionManager _connectionManager;
        private readonly IMatchmakingService _matchmakingService;
        private readonly IGameSessionService _gameSessionService;
        private readonly ILogger<GameHub> _logger;

        public GameHub(
            IConnectionManager connectionManager,
            IMatchmakingService matchmakingService,
            IGameSessionService gameSessionService,
            ILogger<GameHub> logger)
        {
            _connectionManager = connectionManager;
            _matchmakingService = matchmakingService;
            _gameSessionService = gameSessionService;
            _logger = logger;
        }

        public override Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier ?? Context.ConnectionId;
            _connectionManager.AddConnection(userId, Context.ConnectionId);
            _logger.LogInformation("Client connected: {UserId} ({ConnectionId})", userId, Context.ConnectionId);
            return base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier ?? Context.ConnectionId!;

            // 1. If in a match, notify opponent and remove game
            var matchId = _gameSessionService.GetMatchIdForPlayer(userId);
            if (matchId != null)
            {
                var game = _gameSessionService.GetGame(matchId);
                if (game != null)
                {
                    await Clients.Group(matchId).SendAsync("OpponentDisconnected", userId);
                    await Clients.Group(matchId).SendAsync("ReceiveMsg", "Opponent left. You win!");
                    _gameSessionService.RemoveGame(matchId);
                }
            }

            // 2. Remove connection and queue presence
            _connectionManager.RemoveConnection(userId);
            _matchmakingService.RemoveFromQueue(userId);

            _logger.LogInformation("Client disconnected: {UserId}", userId);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task MakeMove(object movedata, string matchId)
        {
            var playerId = Context.UserIdentifier ?? Context.ConnectionId;
            var gameState = _gameSessionService.GetGame(matchId);

            if (gameState is null)
            {
                await Clients.Caller.SendAsync("ReceiveMsg", "Server Error: Game not found");
                return;
            }

            var moveResult = gameState.MakeMove(playerId, movedata);

            // Send board update (to group on success, or caller on rejection)
            var targetClients = moveResult.Success ? Clients.Group(matchId) : Clients.Caller;
            await targetClients.SendAsync("BoardUpdate", gameState.GetBoardState(), gameState.CurrentTurnPlayerId);

            if (moveResult.Success)
            {
                if (gameState.IsGameOver)
                {
                    await Clients.Group(matchId).SendAsync("GameOver", gameState.WinnerId);
                    _gameSessionService.RemoveGame(matchId);
                }
            }
            else
            {
                await Clients.Caller.SendAsync("ReceiveMsg", moveResult.ErrorMessage ?? "Invalid move");
            }
        }

        public async Task SendChatMessage(string matchId, string message)
        {
            var playerId = Context.UserIdentifier ?? Context.ConnectionId;

            // Verify game exists
            var gameState = _gameSessionService.GetGame(matchId);
            if (gameState == null) return;

            await Clients.Group(matchId).SendAsync("ReceiveChat", playerId, message);
        }

        public async Task FindMatch(GameType type)
        {
            var playerId = Context.UserIdentifier ?? Context.ConnectionId;
            var opponentId = _matchmakingService.QueueOrMatch(type, playerId);

            if (opponentId is null)
            {
                await Clients.Caller.SendAsync("ReceiveMsg", "You are on waiting list...");
            }
            else
            {
                var matchId = Guid.NewGuid().ToString();

                // Add both connections to SignalR group
                await Groups.AddToGroupAsync(Context.ConnectionId, matchId);
                if (_connectionManager.TryGetConnectionId(opponentId, out var oppConn))
                {
                    await Groups.AddToGroupAsync(oppConn, matchId);
                }

                _gameSessionService.CreateGame(matchId, type, playerId, opponentId);
                var game = _gameSessionService.GetGame(matchId)!;

                await Clients.Group(matchId).SendAsync("GameStarted", matchId, game.GetBoardState(), game.CurrentTurnPlayerId);
                await Clients.Group(matchId).SendAsync("ReceiveMsg", "Match Found! Game Started.");
            }
        }
    }
}