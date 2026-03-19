using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Client.Services.Hub
{
    public class NotificationService : INotificationService
    {
        private readonly IHubService _hubService;
        private readonly IInboxService _inboxService;
        private readonly IUserListService _userListService;
        
        private string? _lastKvKey = null;

        public event Action<string, Guid> OnMessageNotification;

        public NotificationService(IHubService hubService, IInboxService inboxService, IUserListService userListService)
        {
            _hubService = hubService;
            _inboxService = inboxService;
            _userListService = userListService;

            //Console.WriteLine("NotificationService created");
        }

        public async Task InitializeAsync()
        {
            _hubService.OnMessageNotification -= HandleMessageNotification;
            _hubService.OnNewConversationNotification -= HandleNewConversationNotification;

            _hubService.OnMessageNotification += HandleMessageNotification;
            _hubService.OnNewConversationNotification += HandleNewConversationNotification;
            //Console.WriteLine("NotificationService initialized");
        }


        // the try catch blocks look stupid but they need to be there... cause async void = unhandled exceptions

        private async void HandleMessageNotification(string kvKey)
        {
            //Console.WriteLine($"New message notification");
            try
            {
                if (kvKey == _lastKvKey) return;

                var split = kvKey.Split(':');
                if (split.Length != 3)
                    throw new InvalidOperationException($"Invalid kvKey format: {kvKey}");
                var conversationIdString = split[1];

                if (Guid.TryParse(conversationIdString, out var conversationId))
                {
                    if (_inboxService.ActiveConversationId == conversationId)
                    {
                        OnMessageNotification.Invoke(kvKey, conversationId);
                    }
                    else
                    {
                        await _inboxService.SyncInboxAsync();
                    }
                }
                else throw new InvalidOperationException($"Failed to read conversation id from message key");

                _lastKvKey = kvKey;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{nameof(NotificationService)}: Failed to handle message notification. ERROR: {ex.Message}");
            }
        }

        private async void HandleNewConversationNotification()
        {
            //Console.WriteLine($"New conversation");
            try
            {
                await _inboxService.SyncInboxAsync();
                await _userListService.SyncListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{nameof(HandleNewConversationNotification)} ERROR: {ex.Message}");
            }
        }

        public void Dispose()
        {
            _hubService.OnMessageNotification -= HandleMessageNotification;
            _hubService.OnNewConversationNotification -= HandleNewConversationNotification;
        }

        public void Clear()
        {
            _lastKvKey = null;
            _hubService.OnMessageNotification -= HandleMessageNotification;
            _hubService.OnNewConversationNotification -= HandleNewConversationNotification;
        }
    }
}