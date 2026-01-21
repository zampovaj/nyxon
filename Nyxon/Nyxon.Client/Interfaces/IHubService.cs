using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Client.Interfaces
{
    public interface IHubService
    {
        Task ConnectAsync();
        Task DisconnectAsync();

        Task JoinConversationAsync(Guid conversationId, Guid userId);
        Task JoinAllConversationsAsync(List<Guid> conversationsId, Guid userId);
        Task LeaveConversationAsync(Guid conversationId, Guid userId);

        public void CheckHubState();

        // ui hook
        event Action<string> OnMessageNotification;
        event Action OnNewConversationNotification;
    }
}