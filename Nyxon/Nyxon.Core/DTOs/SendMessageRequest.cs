using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Nyxon.Core.DTOs
{
    public class SendMessageRequest
    {
        [Required]
        [NotNull]
        public Guid ConversationId { get; set; }
        [Required]
        [NotNull]
        [Range(0,int.MaxValue)]
        public int SessionIndex { get; set; }
        [Required]
        [NotNull]
        [Range(0,int.MaxValue)]
        public int MessageIndex { get; set; }
        [Required]
        [NotNull]
        [Range(0,int.MaxValue)]
        public int MessageSequence { get; set; }
        public byte[] EncryptedPayload { get; set; } = Array.Empty<byte>();
    }
}