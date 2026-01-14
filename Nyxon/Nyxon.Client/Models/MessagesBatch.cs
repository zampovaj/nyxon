using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nyxon.Client.Models
{
    public class MessagesBatch
    {
        public List<MessageResponse> Messages { get; set; } = new();
        public int RotationIndex { get; set; }
        public RatchetType RatchetType { get; set; }

        public MessagesBatch(List<MessageResponse> messages, int rotationIndex, RatchetType ratchetType)
        {
            Messages = messages;
            RotationIndex = rotationIndex;
            RatchetType = ratchetType;
        }

        public MessagesBatch(int rotationIndex, RatchetType ratchetType)
        {
            Messages = new();
            RotationIndex = rotationIndex;
            RatchetType = ratchetType;

        }


        public void AddMessage(MessageResponse message)
        {
            if (message != null) Messages.Add(message);
        }

        public override string ToString()
        {
            StringBuilder sb = new();
            sb.AppendLine("MESSAGE BUNDLE:");
            sb.AppendLine($"Rotation index: {RotationIndex}");
            sb.AppendLine($"Ratchet: {RatchetType}");
            foreach (var message in Messages)
            {
                sb.AppendLine($"Message Id:{message.Id} Rotation:{message.SessionIndex} MsgIndex: {message.MessageIndex} EncryptedPayload:{Convert.ToBase64String(message.EncryptedPayload)}");
            }
            return sb.ToString();
        }
    }
}