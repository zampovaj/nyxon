using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Core.DTOs
{
    public class HandshakeDto
    {
        [Required]
        [NotNull]
        public Guid Id { get; set; }
        [Required]
        [NotNull]
        public Guid ConversationId { get; set; }

        [Required]
        [NotNull]
        [MinLength(32)]
        [MaxLength(32)]
        public byte[] PublicEphemeralKey { get; set; }

        [Required]
        [NotNull]
        [MinLength(32)]
        [MaxLength(32)]
        public byte[] PublicIdentityKey { get; set; }
        [Required]
        [NotNull]
        [MinLength(32)]
        [MaxLength(32)]
        public byte[] PrivateSpk { get; set; }
        [Required]
        [NotNull]
        [MinLength(32)]
        [MaxLength(32)]
        public byte[]? PrivateOpk { get; set; }
    }
}