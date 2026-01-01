using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Nyxon.Core.Models.Vaults;

namespace Nyxon.Core.DTOs
{
    public class CreateConversationRequest
    {
        // conversation
        [Required]
        [NotNull]
        public Guid ConversationId { get; set; }
        [Required]
        [NotNull]
        public Guid TargetUserId { get; set; }

        // conversation vault
        [Required]
        [NotNull]
        public ConversationVaultData VaultData { get; set; }

        // handshake
        [Required]
        [NotNull]
        [MinLength(32)]
        [MaxLength(32)]
        public byte[] PublicEphemeralKey { get; set; } = Array.Empty<byte>();
        [Required]
        [NotNull]
        public Guid SpkPublicId { get; set; }
        [Required]
        [NotNull]
        public Guid? OpkPublicId { get; set; }
    }
}