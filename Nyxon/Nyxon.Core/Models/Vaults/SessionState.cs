using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


/*session_state :
current_session_key : key
rotatoin_index : int
message_index : int
rotate_after : int
rotate_after_time : time*/

namespace Nyxon.Core.Models.Vaults
{
    public class SessionState
    {
        public byte[] EncryptedCurrentKey { get; set; }
        public int RotationIndex { get; set; }
        public int MessageIndex { get; set; }
        public int RotateAfter { get; set; }
        public TimeSpan RotateAtTime { get; set; }

        public SessionState() {}

        public SessionState(byte[] encryptedCurrentKey, int rotationIndex, int messageIndex, int rotateAfter, TimeSpan rotateAtTime)
        {
            EncryptedCurrentKey = encryptedCurrentKey;
            RotationIndex = rotationIndex;
            MessageIndex = messageIndex;
            RotateAfter = rotateAfter;
            RotateAtTime = rotateAtTime;
        }

        public SessionState(byte[] encryptedKey)
        {
            EncryptedCurrentKey = encryptedKey;
            RotationIndex = 0;
            MessageIndex = 0;
            RotateAfter = 10;
            RotateAtTime = TimeSpan.FromMinutes(30);
        }
    }
}