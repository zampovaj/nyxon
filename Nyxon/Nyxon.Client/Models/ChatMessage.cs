using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Client.Models
{
    public class ChatMessage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ConversationId { get; set; }

        public string SenderUsername { get; set; } = string.Empty;

        public int SequenceNumber { get; set; }

        // default dots till decryption is done
        public string Content { get; set; } = "••••••";
        public DateTime SentAt { get; set; }

        // ui flags
        public bool IsMine { get; set; }
        public bool IsDecrypted { get; set; }

        // keep in case we need to repeat decryption
        public byte[]? RawPayload { get; set; }
    }
}