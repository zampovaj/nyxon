using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/*    ratchet:
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
    public class RatchetState
    {
        public SessionState Session { get; set; }
        public List<Snapshot> Snapshots { get; set; } = new();

        public RatchetState() {}

        public RatchetState(SessionState session, List<Snapshot> snapshots)
        {
            Session = session;
            Snapshots = snapshots;
        }

        public RatchetState(byte[] encryptedKey)
        {
            Session = new SessionState(encryptedKey);
            Snapshots = new List<Snapshot>() {new Snapshot(encryptedKey)};
        }
    }
}