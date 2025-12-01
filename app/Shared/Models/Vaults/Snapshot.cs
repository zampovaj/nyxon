using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/*snapshots : 
    rotation_index : int / number of rotations done
    session_key : key
    created_at : datetime*/
namespace Shared.Models.Vaults
{
    public class Snapshot
    {
        public int RotationIndex { get; set; }
        public byte[] SessionKey { get; set; }
        public DateTime CreatedAt { get; set; }

        public Snapshot(int rotationIndex, byte[] sessionKey, DateTime createdAt)
        {
            RotationIndex = rotationIndex;
            SessionKey = sessionKey;
            CreatedAt = createdAt;
        }

        public Snapshot(int rotationIndex, byte[] sessionKey)
        {
            RotationIndex = rotationIndex;
            SessionKey = sessionKey;
            CreatedAt = DateTime.UtcNow;
        }
    }
}