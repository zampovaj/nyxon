using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shared.DTOs
{
    public class ConversationVaultDto
    {
        public Guid ConversationId { get; set; }

        public DateTime? UpdatedAt { get; set; }
        public int RecvCounter { get; set; }
        public int SendCounter { get; set; }
        public byte[] VaultBlob { get; set; }

    }
}