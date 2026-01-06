using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
                var username = await _conversationService.OpenConversationAsync(conversationId);
                await ActiveConversation.InitializeAsync(conversationId, username);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                Notify();
            }
        }

        public async Task SendMessageAsync()
        {
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
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                InputString = "";
            }
            finally
            {
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

        private void Notify() => StateChanged?.Invoke();
    }
}