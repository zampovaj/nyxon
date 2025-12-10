using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Client.State
{
    public class AppState
    {
        public UserContext? CurrentUser { get; private set; }

        // dictionary cause o(1) lookup by username
        public Dictionary<string, Conversation> Conversations { get; private set; } = new();

        public bool IsLoggedIn => CurrentUser != null && CurrentUser.IsValid;

        // ui will subscrie to this
        public event Action? OnChange;
        private void NotifyStateChanged() => OnChange?.Invoke();

        // methods for changing state
        public void SetUser(UserContext user)
        {
            CurrentUser = user;
            NotifyStateChanged();
        }

        public void Logout()
        {
            CurrentUser = null;
            Conversations.Clear();
            NotifyStateChanged();
        }

        // finds conversation from list by username
        // if it doesnt exist -> create new conversation
        public Conversation GetOrAddConversation(string targetUsername)
        {
            if (string.IsNullOrWhiteSpace(targetUsername))
                throw new ArgumentNullException("Username cannot be empty");

            if (!Conversations.ContainsKey(targetUsername))
            {
                // basically create a placeholder for the data that will be received from api resposnse
                Conversations[targetUsername] = new Conversation
                {
                    TargetUsername = targetUsername,
                };
            }
            return Conversations[targetUsername];
        }

        // new message created
        public void AddMessage(string targetUsername, ChatMessage message)
        {
            var conversation = GetOrAddConversation(targetUsername);

            // in case a message got sent twice this avoids duplicates
            if (!conversation.Messages.Any(m => m.Id == message.Id))
            {
                conversation.Messages.Add(message);
                conversation.UnreadCount = message.IsMine ? 0 : conversation.UnreadCount + 1;
                NotifyStateChanged();
            }
        }
    }
}