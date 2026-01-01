/*conversation_id : uuid
user_id : uuid
version : small int
updated_at : datetime
recv_counter : int
send_counter : int
vault_blob : bytes // encrypted with user_vaults.vault_key →
    initial_key // exchanged during hanshake
    sending :
        session :
            current_session_key : key
            rotatoin_index : int
            message_index : int
            rotate_after : int
            rotate_at_time : time
        snapshots : 
            rotation_index : int / number of rotations done
            session_key : key
            created_at : datetime
    receiving:
        session :
            current_session_key : key
            rotatoin_index : int
            message_index : int
            rotate_after : int
            rotate_at_time : time
        snapshots : 
            rotation_index : int / number of rotations done
            session_key : key
            created_at : datetime*/

using System.ComponentModel.DataAnnotations.Schema;
using Nyxon.Core.Version;

namespace Nyxon.Server.Models
{
    public class ConversationVault
    {
        public Guid ConversationId { get; set; }
        public virtual Conversation Conversation { get; set; }

        public Guid UserId { get; set; }
        public virtual User User { get; set; }

        public DateTime UpdatedAt { get; set; }
        public short Version { get; set; }
        public int RecvCounter { get; set; }
        public int SendCounter { get; set; }

        [Column(TypeName = "jsonb")]
        public ConversationVaultData VaultData { get; set; }

        protected ConversationVault() { }

        public ConversationVault(Guid conversationId, Guid userId, DateTime updatedAt, short version, int recvCounter, int sendCounter, ConversationVaultData vaultData)
        {
            ConversationId = conversationId;
            UserId = userId;
            UpdatedAt = updatedAt;
            Version = version;
            RecvCounter = recvCounter;
            SendCounter = sendCounter;
            VaultData = vaultData;
        }

        /// <summary>
        /// Creates a brand new conversation vault
        /// </summary>
        public ConversationVault(Guid conversationId, Guid userId, ConversationVaultData vaultData)
        {
            ConversationId = conversationId;
            UserId = userId;
            UpdatedAt = DateTime.UtcNow;
            Version = AppVersion.Current;
            RecvCounter = 0;
            SendCounter = 0;
            VaultData = vaultData;
        }
    }
}