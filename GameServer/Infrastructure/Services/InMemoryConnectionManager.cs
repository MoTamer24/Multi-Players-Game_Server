using System.Collections.Concurrent;
using GameServer.Application.Interfaces;

namespace GameServer.Infrastructure.Services
{
    public class InMemoryConnectionManager : IConnectionManager
    {
        private readonly ConcurrentDictionary<string, string> _userConnections = new();

        public void AddConnection(string userId, string connectionId)
        {
            _userConnections.AddOrUpdate(userId, connectionId, (_, _) => connectionId);
        }

        public void RemoveConnection(string userId)
        {
            _userConnections.TryRemove(userId, out _);
        }

        public bool TryGetConnectionId(string userId, out string connectionId)
        {
            return _userConnections.TryGetValue(userId, out connectionId!);
        }

        public string? GetConnectionId(string userId)
        {
            _userConnections.TryGetValue(userId, out var connId);
            return connId;
        }
    }
}
