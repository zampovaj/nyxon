using System;
using System.Collections.Concurrent;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Nyxon.Server.Hubs
{
    public class ChatHub : Hub
    {
        private static readonly ConcurrentDictionary<string, string> UserConnections = new();

        public async Task JoinConversation(Guid conversationId, string userIdString)
        {
            UserConnections[userIdString] = Context.ConnectionId;
            await Groups.AddToGroupAsync(Context.ConnectionId, conversationId.ToString());
        }
        public async Task LeaveConversation(Guid conversationId, string userIdString)
        {
            UserConnections.TryRemove(userIdString, out _);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId.ToString());
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            var kvp = UserConnections.FirstOrDefault(x => x.Value == Context.ConnectionId);
            if (!string.IsNullOrEmpty(kvp.Key))
                UserConnections.TryRemove(kvp.Key, out _);

            return base.OnDisconnectedAsync(exception);
        }

        public static bool TryGetConnection(string userId, out string connectionId)
        {
            return UserConnections.TryGetValue(userId, out connectionId);
        }
    }
}