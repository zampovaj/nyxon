using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Nyxon.Core.Models.Vaults;

namespace Nyxon.Core.DTOs
{
    public class MessagesBundleDto
    {
        [Required]
        [NotNull]
        public List<MessageResponse> Messages { get; set; } = new();
        [Required]
        [NotNull]
        public SnapshotsBundleDto Snapshots {get;set;}

        public MessagesBundleDto(List<MessageResponse> messages, SnapshotsBundleDto snapshots)
        {
            Messages = messages;
            Snapshots = snapshots;
        }
    }
}