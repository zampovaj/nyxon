using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Core.DTOs
{
    public class MessageResponse
    {
        [Required]
        [NotNull]
        public Guid Id { get; set; }
        [Required]
        [NotNull]
        public int SequenceNumber { get; set; }
        [Required]
        [NotNull]
        public Guid SenderId { get; set; }
        [Required]
        [NotNull]
        public string SenderUsername { get; set; }
        [Required]
        [NotNull]
        public int SessionIndex { get; set; }
        [Required]
        [NotNull]
        public int MessageIndex { get; set; }
        [Required]
        [NotNull]
        public DateTime CreatedAt { get; set; }
        [Required]
        [NotNull]
        public byte[] EncryptedPayload { get; set; } = Array.Empty<byte>();
    }
}