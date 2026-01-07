using Microsoft.AspNetCore.Components.Web;

namespace Nyxon.Client.ViewModels
{
    public class ChatViewModel : IDisposable
    {
        private readonly IConversationService _conversationService;
        private readonly IActiveConversationService _activeConversationService;

        public ActiveConversation ActiveConversation { get; private set; } = new ActiveConversation();

        public string InputString { get; set; } = "";
        public string? ErrorMessage { get; private set; } = "";
        public event Action? StateChanged;
        public bool IsBusy { get; private set; } = false;

        public ChatViewModel(IConversationService conversationService,
            IActiveConversationService activeConversationService)
        {
            _conversationService = conversationService;
            _activeConversationService = activeConversationService;
        }

        public async Task InitializeAsync(Guid conversationId)
        {
            try
            {
                IsBusy = true;
                var username = await _conversationService.OpenConversationAsync(conversationId);
                await ActiveConversation.InitializeAsync(conversationId, username);
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
                    throw new Exception("Message failed");

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

        public void Dispose()
        {
            InputString = "";
            ErrorMessage = "";
            _activeConversationService.Clear();
            Notify();
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

        private void Notify() => StateChanged?.Invoke();
    }
}