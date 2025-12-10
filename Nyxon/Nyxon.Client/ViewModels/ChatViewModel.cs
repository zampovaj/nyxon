using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Client.ViewModels
{
    // need idisposable to prevent memory leaks
    public class ChatViewModel : IDisposable
    {
        private readonly AppState _appState;

        public string CurrentContactName { get; private set; } = string.Empty;
        public List<ChatMessage> Messages { get; private set; } = new();
        public string NewMessageInput { get; set; } = string.Empty;

        public ChatViewModel(AppState appState)
        {
            _appState = appState;
            _appState.OnChange += OnStateChanged;
        }

        public void SendMessage()
        {
            if (string.IsNullOrWhiteSpace(NewMessageInput) || string.IsNullOrEmpty(CurrentContactName))
                return;

            var newMsg = new ChatMessage
            {
                ConversationId = Guid.Empty, // temporary will be replaced by actual guid from server
                SenderUsername = _appState.CurrentUser?.Username ?? "Me",
                Content = NewMessageInput,
                SentAt = DateTime.Now,
                IsMine = true,
                IsDecrypted = true
            };

            //push to state
            _appState.AddMessage(CurrentContactName, newMsg);

            //clear newmessage
            NewMessageInput = string.Empty;

            //TODO: actaully contact api
        }

        // fetches messages for conversation form state
        private void RefreshMessages()
        {
            if (string.IsNullOrEmpty(CurrentContactName)) return;

            var conversation = _appState.GetOrAddConversation(CurrentContactName);
            Messages = conversation.Messages;
        }

        private void OnStateChanged()
        {
            RefreshMessages();
        }

        public void Dispose()
        {
            _appState.OnChange -= OnStateChanged;
        }
    }
}