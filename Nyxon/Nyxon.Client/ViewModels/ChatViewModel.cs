using Microsoft.AspNetCore.Components.Web;

namespace Nyxon.Client.ViewModels
{
    public class ChatViewModel : IAsyncDisposable
    {
        private readonly IConversationService _conversationService;
        private readonly IActiveConversationService _activeConversationService;
        private readonly IHubService _hubService;
        private readonly UserContext _userContext;
        private readonly IInboxService _inboxService;

        public ActiveConversation ActiveConversation { get; private set; } = new ActiveConversation();

        public string InputString { get; set; } = "";
        public string? ErrorMessage { get; private set; } = "";
        public event Action? StateChanged;
        public bool CanLoadHistory => _isInitialized && !_isLoadingHistory && _hasMoreHistory;
        public bool InitialScrollToBottom = true;

        private readonly SemaphoreSlim _messageLock = new(1, 1);
        private readonly SemaphoreSlim _historyLock = new(1, 1);
        private bool _isInitialized = false;
        private bool _isLoadingHistory = false;
        private bool _hasMoreHistory = true;

        private readonly TaskCompletionSource _initializationCompletion = new(TaskCreationOptions.RunContinuationsAsynchronously);


        public ChatViewModel(IConversationService conversationService,
            IActiveConversationService activeConversationService,
            IHubService hubService,
            UserContext userContext,
            IInboxService inboxService)
        {
            _conversationService = conversationService;
            _activeConversationService = activeConversationService;
            _hubService = hubService;
            _userContext = userContext;
            _inboxService = inboxService;
        }

        public async Task InitializeAsync(Guid conversationId)
        {

            try
            {
                _hubService.OnMessageNotification += HandleMessageNotification;
                _activeConversationService.MessagesDecrypted += OnMessageBundleLoadedNotification;
                var username = await _conversationService.OpenConversationAsync(conversationId);
                await ActiveConversation.InitializeAsync(conversationId, username);
                await _hubService.ConnectAsync();
                await _hubService.JoinConversationAsync(conversationId, (Guid)_userContext.UserId);
                await _inboxService.ReadConversationAsync(conversationId);
                // read history
                await _activeConversationService.LoadRecentMessagesAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                _isInitialized = true;
                _initializationCompletion.TrySetResult();
                _hasMoreHistory = ActiveConversation.Messages.Count == 0 ||
                    ActiveConversation.Messages.Min(m => m.SequenceNumber) > 1;
                InitialScrollToBottom = true;
                Notify();
            }
        }

        public async Task LoadHistoryAsync()
        {
            if (!_isInitialized) return;
            if (!_hasMoreHistory) return;

            await _initializationCompletion.Task;
            await _historyLock.WaitAsync();

            if (_isLoadingHistory)
            {
                _historyLock.Release();
                return;
            }

            _isLoadingHistory = true;

            try
            {
                int lastSequenceNumber = ActiveConversation.Messages
                    .Min(m => m.SequenceNumber);

                await _activeConversationService.LoadHistoryAsync(lastSequenceNumber);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during loading message history: {ex.Message}");
            }
            finally
            {
                _historyLock.Release();
                _isLoadingHistory = false;
                _hasMoreHistory = ActiveConversation.Messages.Min(m => m.SequenceNumber) > 1;
                Notify();
            }
        }

        public async Task SendMessageAsync()
        {
            await _initializationCompletion.Task;
            await _messageLock.WaitAsync();

            try
            {
                ErrorMessage = "";
                if (!IsInputSafe(InputString))
                {
                    Notify();
                    return;
                }

                var message = await _activeConversationService.SendMessageAsync(InputString);
                if (message == null)
                    throw new Exception("Sensding message failed silently.");

                await ActiveConversation.AddMyMessageAsync(message);
                await _inboxService.UpdateConversationAsync(message.SentAt, true, (Guid)ActiveConversation.ConversationId);
                InputString = "";
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                InputString = "";
            }
            finally
            {
                _messageLock.Release();
                Notify();
            }
        }

        public async ValueTask DisposeAsync()
        {
            InputString = "";
            ErrorMessage = "";
            ActiveConversation.Clear();
            if (_activeConversationService.ConversationId != null)
                await _hubService.LeaveConversationAsync((Guid)_activeConversationService.ConversationId, (Guid)_userContext.UserId);
            await _hubService.DisconnectAsync();
            _hubService.OnMessageNotification -= HandleMessageNotification;
            _activeConversationService.MessagesDecrypted -= OnMessageBundleLoadedNotification;
            _activeConversationService.Clear();
            Notify();
        }

        private async void OnMessageBundleLoadedNotification(List<ChatMessage> messages)
        {
            try
            {
                await ActiveConversation.AddNewMessagesBundle(messages);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Loading of messages failed: {ex.Message}");
            }
        }

        private async void HandleMessageNotification(string kvKey)
        {
            Console.WriteLine("Reached the handler");
            await _initializationCompletion.Task;
            await _messageLock.WaitAsync();

            try
            {
                Console.WriteLine("Passed the sempahore");
                var split = kvKey.Split(':');
                if (split.Length != 3)
                    throw new InvalidOperationException($"Invalid kvKey format: {kvKey}");
                var conversationIdString = split[1];

                if (Guid.TryParse(conversationIdString, out var conversationId))
                {
                    if (_activeConversationService.ConversationId == conversationId &&
                        ActiveConversation.ConversationId == conversationId)
                    {
                        var newMessage = await _activeConversationService.ReceiveMessageAsync(kvKey);
                        if (newMessage == null)
                            throw new Exception("Receiving message failed silently.");

                        await ActiveConversation.AddNewMessageAsync(newMessage);
                        await _inboxService.UpdateConversationAsync(newMessage.SentAt, true, conversationId);
                    }
                    else
                    {
                        await _inboxService.UpdateConversationAsync(DateTime.UtcNow, false, conversationId);
                    }
                }
                else throw new InvalidOperationException($"Failed to read conversation id from message key");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing message notification: {ex.Message}");
            }
            finally
            {
                _messageLock.Release();
                Notify();
            }
        }

        private bool IsInputSafe(string input)
        {
            if (string.IsNullOrEmpty(input) || !input.Any())
            {
                ErrorMessage = "Message can't be empty.";
                return false;
            }
            if (input.Length > 8192)
            {
                ErrorMessage = "Message is too long. Maximum allowed length is 1024 characters";
                return false;
            }

            return true;
        }
        public async Task HandleEnterKey(KeyboardEventArgs e)
        {
            if (e.Key != "Enter")
                return;

            await SendMessageAsync();
        }

        private void Notify() => StateChanged?.Invoke();
    }
}