using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


/*vault_blob : bytes // encrypted with user_vaults.vault_key →
    initial_key // exchanged during hanshake
    sending :
        session :
            current_session_key : key
            rotation_index : int
            message_index : int
            rotate_after : int
            rotate_at_time : time
        snapshots : 
            rotation_index : int / number of rotations done
            session_key : key
            created_at : datetime
    receiving :
        session :
            current_session_key : key
            rotation_index : int
            message_index : int
            rotate_after : int
            rotate_at_time : time
        snapshots : 
            rotation_index : int / number of rotations done
            session_key : key
            created_at : datetime*/

namespace Nyxon.Core.Models.Vaults
{
    public class ConversationVaultData
    {
        public byte[] EncryptedRootKey { get; set; }
        public RatchetState Sending { get; set; }
        public RatchetState Receiving { get; set; }

        public ConversationVaultData() { }

        public ConversationVaultData(byte[] encryptedRootKey, RatchetState sending, RatchetState receiving)
        {
            EncryptedRootKey = encryptedRootKey;
            Sending = sending;
            Receiving = receiving;
        }

        // TODO: check the constructor if its actually okay for production this was just mock for dev
        public ConversationVaultData(byte[] encryptedRootKey, byte[] encryptedSendingKey, byte[] encryptedReceivingKey)
        {
            EncryptedRootKey = encryptedRootKey;
            Sending = new RatchetState(encryptedSendingKey);
            Receiving = new RatchetState(encryptedReceivingKey);
        }
    }
}