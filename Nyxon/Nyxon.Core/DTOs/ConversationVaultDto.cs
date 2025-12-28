using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Core.DTOs
{
    public class ConversationVaultDto
    {
        [Required]
        [NotNull]
        public Guid ConversationId { get; set; }
        [Required]
        // TODO: try to remmeber why is this nullable
        public DateTime? UpdatedAt { get; set; }
        [Required]
        [NotNull]
        public int RecvCounter { get; set; }
        [Required]
        [NotNull]
        public int SendCounter { get; set; }
        [Required]
        [NotNull]
        public byte[] VaultBlob { get; set; }
    }
}