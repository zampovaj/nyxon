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
        public byte[] CurrentSessionKey { get; set; }
        public int RotationIndex { get; set; }
        public int MessageIndex { get; set; }
        public int RotateAfter { get; set; }
        public TimeSpan RotateAtTime { get; set; }

        public SessionState() {}

        public SessionState(byte[] currentSessionKey, int rotationIndex, int messageIndex, int rotateAfter, TimeSpan rotateAtTime)
        {
            CurrentSessionKey = currentSessionKey;
            RotationIndex = rotationIndex;
            MessageIndex = messageIndex;
            RotateAfter = rotateAfter;
            RotateAtTime = rotateAtTime;
        }
    }
}