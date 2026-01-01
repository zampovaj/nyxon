using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/*snapshots : 
    rotation_index : int / number of rotations done
    session_key : key
    created_at : datetime*/
namespace Nyxon.Core.Models.Vaults
{
    public class Snapshot
    {
        public int RotationIndex { get; set; }
        public byte[] EncryptedSessionKey { get; set; }
        public DateTime CreatedAt { get; set; }

        public Snapshot() { }

        public Snapshot(int rotationIndex, byte[] encryptedSessionKey, DateTime createdAt)
        {
            RotationIndex = rotationIndex;
            EncryptedSessionKey = encryptedSessionKey;
            CreatedAt = createdAt;
        }

        public Snapshot(int rotationIndex, byte[] encryptedSessionKey)
        {
            RotationIndex = rotationIndex;
            EncryptedSessionKey = encryptedSessionKey;
            CreatedAt = DateTime.UtcNow;
        }

        public Snapshot(byte[] encrpytedKey)
        {
            RotationIndex = 0;
            EncryptedSessionKey = encrpytedKey;
            CreatedAt = DateTime.UtcNow;
        }
    }
}