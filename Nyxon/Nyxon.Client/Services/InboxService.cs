using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Client.Services
{
    public class InboxService : IInboxService
    {
        private readonly IConversationRepository _repository;
        public List<Conversation> Conversations { get; private set; } = new();

        public InboxService(IConversationRepository repository)
        {
            _repository = repository;
        }

        public event Action OnChange;

        public async Task SyncInboxAsync()
        {
            try
            {
                var dtos = await _repository.FetchInboxAsync();

                if (dtos == null) return;

                Conversations = dtos.Select(d => new Conversation()
                {
                    ConversationId = d.Id,
                    TargetUsername = d.Username,
                    LastMessageAt = d.LastMessageAt,
                    HasUnreadMessages = d.HasUnreadMessages
                }).ToList();

                NotifyStateChanged();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Inbox sync failed: {ex.Message}");
            }
        }

        public void Clear()
        {
            Conversations.Clear();
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}