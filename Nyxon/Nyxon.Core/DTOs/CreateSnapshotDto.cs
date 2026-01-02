using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Core.DTOs
{
    public class CreateSnapshotDto
    {
        public enum RatchetType
        {
            Sending,
            Receiving
        };
        public Guid ConversationId { get; set; }
        public RatchetType Type { get; set; }
        public int RotationIndex { get; set; }
        public byte[] EncryptedSessionKey { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}