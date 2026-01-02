using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/*messages:{conversation_id}:{message_sequence} → JSON: {
    message_id : uuid
    sender_username : string
    session_index : int
    message_index : int
    message_index : int
    created_at : datetime
    encrypted_payload : bytes}*/

namespace Nyxon.Server.Models.Valkey
{
    public class Message
    {
        public Guid Id { get; set; }
        public int SequenceNumber { get; set; }
        public string SenderUsername { get; set; }
        public Guid SenderId { get; set; }
        public int SessionIndex { get; set; }
        public int MessageIndex { get; set; }
        public DateTime CreatedAt { get; set; }
        public byte[] EncryptedPayload { get; set; }

        public Message() { }

        public Message(Guid id, int sequenceNumber, Guid senderId, string senderUsername, int sessionIndex, int messageIndex, DateTime createdAt, byte[] encryptedPayload)
        {
            Id = id;
            SequenceNumber = sequenceNumber;
            SenderId = senderId;
            SenderUsername = senderUsername;
            SessionIndex = sessionIndex;
            MessageIndex = messageIndex;
            CreatedAt = createdAt;
            EncryptedPayload = encryptedPayload;
        }

        public Message(int sequenceNumber, Guid senderId, string senderUsername, int sessionIndex, int messageIndex, byte[] encryptedPayload)
        {
            Id = Guid.NewGuid();
            SequenceNumber = sequenceNumber;
            SenderId = senderId;
            SenderUsername = senderUsername;
            SessionIndex = sessionIndex;
            MessageIndex = messageIndex;
            CreatedAt = DateTime.UtcNow;
            EncryptedPayload = encryptedPayload;
        }
    }
}