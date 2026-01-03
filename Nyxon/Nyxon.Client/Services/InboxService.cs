using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Client.Services
{
    public class InboxService : IInboxService
    {
        private readonly IConversationRepository _repository;
        private readonly IHandshakeService _handshakeService;
        private readonly UserContext _userContext;

        public List<Conversation> Conversations { get; private set; } = new();

        public InboxService(IConversationRepository repository,
        IHandshakeService handshakeService,
        UserContext userContext)
        {
            _repository = repository;
            _handshakeService = handshakeService;
            _userContext = userContext;

            _handshakeService.OnChange += HandshakeChanged;
        }

        public event Action OnChange;

        public async Task SyncInboxAsync()
        {
            if (!_userContext.IsAuthenticated)
                return;

            try
            {
                var inbox = await _repository.FetchInboxAsync();

                if (inbox == null) return;

                Conversations = inbox.Conversations
                    .Select(d => new Conversation()
                    {
                        ConversationId = d.Id,
                        TargetUsername = d.Username,
                        LastMessageAt = d.LastMessageAt,
                        HasUnreadMessages = d.HasUnreadMessages,
                        HandshakeId = inbox.Handshakes
                            .Where(h => h.ConversationId == d.Id)
                            .Select(h => (Guid?)h.Id)
                            .FirstOrDefault()
                    }).ToList();

                await _handshakeService.LoadHandshakesAsync(inbox.Handshakes);

                NotifyStateChanged();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Inbox sync failed: {ex.Message}");
            }
        }

        public async Task<Guid?> GetConversationAsync(string username)
        {
            return Conversations
                .Where(c => c.TargetUsername == username)
                .Select(c => c.ConversationId)
                .FirstOrDefault();
        }

        public void Clear()
        {
            Conversations.Clear();
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
        private void HandshakeChanged()
        {
            _ = OnHandshakesChangedAsync();
        }
        private async Task OnHandshakesChangedAsync()
        {
            await SyncInboxAsync();
        }

    }
}