using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Core.DTOs
{
    public class MessagesBundle
    {
        public List<MessageResponse> Messages { get; set; }
        public SnapshotsDto Snapshots { get; set; }

        public MessagesBundle(List<MessageResponse> messages, SnapshotsDto snapshots)
        {
            Messages = messages;
            Snapshots = snapshots;
        }
    }
}