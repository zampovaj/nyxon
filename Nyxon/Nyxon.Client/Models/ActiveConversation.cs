using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nyxon.Client.Pages;
using Org.BouncyCastle.Asn1.X509;

namespace Nyxon.Client.Models
{
    public class ActiveConversation
    {
        public Guid? ConversationId { get; set; }
        public string TargetUsername { get; set; } = "";
        public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
        // for dupliacte protection in O(1)
        private HashSet<Guid> messageIds = new();

        public async Task InitializeAsync(Guid conversationId, string targetUsername)
        {
            ConversationId = conversationId;
            TargetUsername = targetUsername;
        }

        public async Task AddNewMessagesBundle(List<ChatMessage> messages)
        {
            // TODO: this cant be right lol
            foreach (var message in messages)
            {
                if (message.IsMine) message.SenderUsername = "Me";
                try
                {
                    await AddNewMessageAsync(message);
                }
                catch { }
            }
        }

        public async Task AddNewMessageAsync(ChatMessage message)
        {
            if (message.ConversationId != ConversationId)
                throw new InvalidOperationException("Message conversation id doesn't match this conversation");

            await InsertMessageAsync(message);
        }

        public async Task AddMyMessageAsync(NewMessageObject newMessage)
        {
            await InsertMessageAsync(new ChatMessage()
            {
                Id = newMessage.Id,
                ConversationId = (Guid)ConversationId,
                SenderUsername = "Me",
                SequenceNumber = newMessage.SequenceNumber,
                Content = newMessage.Content,
                SentAt = newMessage.SentAt,
                IsMine = true
            });
        }

        private async Task InsertMessageAsync(ChatMessage message)
        {
            //compare for duplicates
            if (!messageIds.Add(message.Id))
                return;

            // Messages[^1] very clever thing from dotnet that gives you Messages[n-1]
            // works with other numbers too: [^2] -> [n-2],...
            if (Messages.Count() == 0 || message.SequenceNumber > Messages[^1].SequenceNumber)
            {
                Messages.Add(message);
                return;
            }

            // if message is out of order, use binary search to find the exact insertion spot in O(log N)
            var index = Messages.BinarySearch(message, new MessageSequenceComparer());

            if (index < 0)
            {
                // binary search returns negative operator as the message inseriton index
                // flip it back
                index = ~index;

                Messages.Insert(index, message);
            }
        }
        public void Clear()
        {
            Messages.Clear();
            messageIds.Clear();
            ConversationId = null;
            TargetUsername = "";
        }
    }
    // required for binary search to work
    public class MessageSequenceComparer : IComparer<ChatMessage>
    {
        public int Compare(ChatMessage? x, ChatMessage? y)
        {
            if (x == null || y == null) return 0;
            return x.SequenceNumber.CompareTo(y.SequenceNumber);
        }
    }
}