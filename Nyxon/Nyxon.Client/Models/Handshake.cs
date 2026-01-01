using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Client.Models
{
    public class Handshake
    {
        public Guid Id { get; set; }
        public Guid ConversationId { get; set; }
        public byte[] PublicEphemeralKey { get; set; }
        public byte[] PublicIdentityKey { get; set; }
        public byte[] PrivateSpk { get; set; }
        public byte[]? PrivateOpk { get; set; }
        public bool IsProcessing { get; set; } = false;

        public Handshake(Guid id, Guid conversationId, byte[] publicEphemeralKey, byte[] publicIdentityKey, byte[] privateSpk, byte[]? privateOpk = null)
        {
            Id = id;
            ConversationId = conversationId;
            PublicEphemeralKey = publicEphemeralKey;
            PublicIdentityKey = publicIdentityKey;
            PrivateSpk = privateSpk;
            PrivateOpk = privateOpk;
        }
    }
}