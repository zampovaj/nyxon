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
        public Guid Id { get; set; }
        public int RotationIndex { get; set; }
        public byte[] EncryptedSessionKey { get; set; }
        public DateTime CreatedAt { get; set; }

        public Snapshot() { }

        public Snapshot(int rotationIndex, byte[] encryptedSessionKey, DateTime createdAt)
        {
            Id = Guid.NewGuid();
            RotationIndex = rotationIndex;
            EncryptedSessionKey = encryptedSessionKey;
            CreatedAt = createdAt;
        }

        public Snapshot(Guid id, int rotationIndex, byte[] encryptedSessionKey, DateTime createdAt)
        {
            Id = id;
            RotationIndex = rotationIndex;
            EncryptedSessionKey = encryptedSessionKey;
            CreatedAt = createdAt;
        }

        public Snapshot(int rotationIndex, byte[] encryptedSessionKey)
        {
            Id = Guid.NewGuid();
            RotationIndex = rotationIndex;
            EncryptedSessionKey = encryptedSessionKey;
            CreatedAt = DateTime.UtcNow;
        }

        public Snapshot(Guid id, int rotationIndex, byte[] encryptedSessionKey)
        {
            Id = id;
            RotationIndex = rotationIndex;
            EncryptedSessionKey = encryptedSessionKey;
            CreatedAt = DateTime.UtcNow;
        }

        public Snapshot(byte[] encryptedKey)
        {
            Id = Guid.NewGuid();
            RotationIndex = 0;
            EncryptedSessionKey = encryptedKey;
            CreatedAt = DateTime.UtcNow;
        }
    }
}