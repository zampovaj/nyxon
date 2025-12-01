using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shared.Version;
using Npgsql.Replication;

/*id : uuid pk ai
user_id : references users[user_id]
type : string
public_key : bytes
encrypted_key : bytes
version : small int
used : bool*/

namespace Backend.Models
{
    public class Prekeys
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public virtual User User { get; set; }

        public string Type { get; set; }
        public byte[] PublicKey { get; set; }
        public byte[] EncryptedKey { get; set; }
        public DateTime CreatedAt { get; set; }
        public short Version { get; set; }
        public bool Used { get; private set; }

        protected Prekeys() { }

        public Prekeys(Guid id, Guid userId, string type, byte[] publicKey, byte[] encryptedKey, DateTime createdAt, short version, bool used)
        {
            Id = id;
            UserId = userId;
            Type = type;
            PublicKey = publicKey;
            EncryptedKey = encryptedKey;
            CreatedAt = createdAt;
            Version = version;
            Used = used;
        }

        /// <summary>
        /// Creates a brand new prekey
        /// </summary>
        public Prekeys(Guid userId, string type, byte[] publicKey, byte[] encryptedKey)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            Type = type;
            PublicKey = publicKey;
            EncryptedKey = encryptedKey;
            CreatedAt = DateTime.UtcNow;
            Version = AppVersion.Current;
            Used = false;
        }

        public void Use()
        {
            Used = true;
        }
    }
}