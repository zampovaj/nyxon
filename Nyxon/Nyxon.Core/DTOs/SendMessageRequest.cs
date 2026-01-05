using System.Diagnostics.CodeAnalysis;
using Nyxon.Core.Models.Vaults;

namespace Nyxon.Core.DTOs
{
    public class SendMessageRequest
    {
        [Required]
        [NotNull]
        public Guid ConversationId { get; set; }
        [Required]
        [NotNull]
        [Range(0, int.MaxValue)]
        public int SessionIndex { get; set; }
        [Required]
        [NotNull]
        [Range(0, int.MaxValue)]
        public int MessageIndex { get; set; }
        [Required]
        [NotNull]
        [Range(0, int.MaxValue)]
        public int MessageSequence { get; set; }
        public byte[] EncryptedPayload { get; set; } = Array.Empty<byte>();

        // ratchet
        public byte[]? EncryptedCurrentSessionKey { get; set; }

        // snapshot
        public Snapshot? Snapshot { get; set; }
    }
}