using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shared.DTOs
{
    public class MessageResponse
    {
        public Guid Id { get; set; }
        public int SequenceNumber { get; set; }
        public string SenderUsername { get; set; }
        public int SessionIndex { get; set; }
        public int MessageIndex { get; set; }
        public DateTime CreatedAt { get; set; }
        public byte[] EncryptedPayload { get; set; } = Array.Empty<byte>();
    }
}