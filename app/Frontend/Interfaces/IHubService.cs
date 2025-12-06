using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Frontend.Interfaces
{
    public interface IHubService
    {
        Task ConnectAsync();
        Task DisconnectAsync();

        Task JoinConversationAsync(Guid conversationId);
        Task LeaveConversationAsync(Guid conversationId);

        // ui hook
        event Action<string> OnMessageNotification;
    }
}