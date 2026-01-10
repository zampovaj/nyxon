using Microsoft.AspNetCore.Components.Web;

namespace Nyxon.Client.ViewModels
{
    public class ChatViewModel : IAsyncDisposable
    {
        private readonly IConversationService _conversationService;
        private readonly IActiveConversationService _activeConversationService;
        private readonly IHubService _hubService;
        private readonly UserContext _userContext;
        private readonly IJSRuntime _jSRuntime;

        public ActiveConversation ActiveConversation { get; private set; } = new ActiveConversation();

        public string InputString { get; set; } = "";
        public string? ErrorMessage { get; private set; } = "";
        public event Action? StateChanged;
        public bool IsBusy { get; private set; } = false;

        public ChatViewModel(IConversationService conversationService,
            IActiveConversationService activeConversationService,
            IHubService hubService,
            UserContext userContext,
            IJSRuntime jSRuntime)
        {
            _conversationService = conversationService;
            _activeConversationService = activeConversationService;
            _hubService = hubService;
            _userContext = userContext;
            _jSRuntime = jSRuntime;
        }

        public async Task InitializeAsync(Guid conversationId)
        {
            try
            {
                IsBusy = true;
                _hubService.OnMessageNotification += HandleMessageNotification;
                var username = await _conversationService.OpenConversationAsync(conversationId);
                await ActiveConversation.InitializeAsync(conversationId, username);
                await _hubService.ConnectAsync();
                await _hubService.JoinConversationAsync(conversationId, (Guid)_userContext.UserId);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsBusy = false;
                Notify();
            }
        }

        public async Task SendMessageAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            ErrorMessage = "";

            if (!IsInputSafe(InputString))
            {
                Notify();
                return;
            }
            try
            {
                var message = await _activeConversationService.SendMessageAsync(InputString);
                if (message == null)
                    throw new Exception("Sensding message failed silently.");

                await ActiveConversation.AddMyMessageAsync(message);
                InputString = "";
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                InputString = "";
            }
            finally
            {
                IsBusy = false;
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
            _activeConversationService.Clear();
            Notify();
        }

        private async void HandleMessageNotification(string kvKey)
        {
            try
            {
                if (_activeConversationService.ConversationId == ActiveConversation.ConversationId)
                {
                    var newMessage = await _activeConversationService.ReceiveMessageAsync(kvKey);
                    if (newMessage == null)
                        throw new Exception("Receiving message failed silently.");

                    await ActiveConversation.AddNewMessageAsync(newMessage);
                    Notify();

                    await ScrollIfNeededAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing message notification: {ex.Message}");
            }
        }

        private bool IsInputSafe(string input)
        {
            if (string.IsNullOrEmpty(input) || !input.Any())
            {
                ErrorMessage = "Message can't be empty.";
                return false;
            }
            if (input.Length > 1024)
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
        private async Task ScrollIfNeededAsync()
        {
            try
            {
                bool atBottom = await _jSRuntime.InvokeAsync<bool>("chatScroll.isUserAtBottom", "messagesContainer");
                if (atBottom)
                {
                    await _jSRuntime.InvokeVoidAsync("chatScroll.scrollToBottom", "messagesContainer");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Scroll error: {ex.Message}");
            }
        }


        private void Notify() => StateChanged?.Invoke();
    }
}