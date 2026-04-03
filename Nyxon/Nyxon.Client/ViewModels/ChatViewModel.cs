using Microsoft.AspNetCore.Components.Web;

namespace Nyxon.Client.ViewModels
{
    public class ChatViewModel : IAsyncDisposable
    {
        private readonly IConversationService _conversationService;
        private readonly IActiveConversationService _activeConversationService;
        private readonly INotificationService _notificationService;
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
        public bool IsTargetDeleted => ActiveConversation.TargetUsername == AccountConstants.DeletedAccount;

        private readonly TaskCompletionSource _initializationCompletion = new(TaskCreationOptions.RunContinuationsAsynchronously);


        public ChatViewModel(IConversationService conversationService,
            IActiveConversationService activeConversationService,
            INotificationService notificationService,
            UserContext userContext,
            IInboxService inboxService)
        {
            _conversationService = conversationService;
            _activeConversationService = activeConversationService;
            _notificationService = notificationService;
            _userContext = userContext;
            _inboxService = inboxService;
        }

        public async Task InitializeAsync(Guid conversationId)
        {

            try
            {
                _notificationService.OnMessageNotification += HandleMessageNotification;
                _activeConversationService.MessagesDecrypted += OnMessageBundleLoadedNotification;
                var username = await _conversationService.OpenConversationAsync(conversationId);
                await ActiveConversation.InitializeAsync(conversationId, username);
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
                await _inboxService.SetSelectedAsync((Guid)ActiveConversation.ConversationId);
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
            if (IsTargetDeleted) return;

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
                    throw new Exception("Sending message failed silently.");

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
            _notificationService.OnMessageNotification -= HandleMessageNotification;
            _activeConversationService.MessagesDecrypted -= OnMessageBundleLoadedNotification;
            _activeConversationService.Clear();
            _inboxService.Unselect();
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

        private async void HandleMessageNotification(string kvKey, Guid conversationId)
        {
            await _initializationCompletion.Task;
            await _messageLock.WaitAsync();

            try
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
                ErrorMessage = "Message is too long. Maximum allowed length is 8192 characters";
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