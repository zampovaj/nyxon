using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


/*vault_blob : bytes // encrypted with user_vaults.vault_key →
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

namespace Nyxon.Core.Models.Vaults
{
    public class ConversationBlob
    {
        public byte[] InitialKey { get; set; }
        public RatchetState Sending { get; set; }
        public RatchetState Receiving { get; set; }

        public ConversationBlob() {}

        public ConversationBlob(byte[] initialKey, RatchetState sending, RatchetState receiving)
        {
            InitialKey = initialKey;
            Sending = sending;
            Receiving = receiving;
        }
    }
}