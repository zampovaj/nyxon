using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Nyxon.Core.Models.Vaults;

namespace Nyxon.Core.DTOs
{
    public class MessagesBundleResponse
    {
        [Required]
        [NotNull]
        public List<MessageResponse> Messages { get; set; } = new();
        [Required]
        [NotNull]
        public SnapshotsDto Snapshots { get; set; }

        public MessagesBundleResponse(List<MessageResponse> messages, SnapshotsDto snapshots)
        {
            Messages = messages;
            Snapshots = snapshots;
        }
    }
}