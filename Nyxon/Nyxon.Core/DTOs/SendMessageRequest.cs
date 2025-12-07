using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Core.DTOs
{
    public class SendMessageRequest
    {
        public Guid ConversationId { get; set; }
        public int SessionIndex { get; set; }
        public int MessageIndex { get; set; }
        public int MessageSequence { get; set; }
        public byte[] EncryptedPayload { get; set; } = Array.Empty<byte>();
    }
}