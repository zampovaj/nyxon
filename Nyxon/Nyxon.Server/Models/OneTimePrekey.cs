using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// id            : uuid pk
// user_id       : references users(id)
// public_key    : bytea
// encrypted_key : bytea
// version       : smallint
// used          : bool default false

namespace Nyxon.Server.Models
{
    public class OneTimePrekey
    {

        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public virtual User User { get; set; }
        public byte[] PublicKey { get; set; }
        public byte[] EncryptedKey { get; set; }
        public bool Used { get; set; }
        public short Version { get; private set; }

        protected OneTimePrekey() { }

        public OneTimePrekey(Guid id, Guid userId, byte[] publicKey, byte[] encryptedKey, bool used, short version)
        {
            Id = id;
            UserId = userId;
            PublicKey = publicKey;
            EncryptedKey = encryptedKey;
            Used = used;
            Version = version;
        }

        /// <summary>
        /// Creates a new signed prekey
        /// </summary>
        public OneTimePrekey(Guid userId, byte[] publicKey, byte[] encryptedKey)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            PublicKey = publicKey;
            EncryptedKey = encryptedKey;
            Used = false;
            Version = AppVersion.Current;
        }

        public void Use()
        {
            Used = false;
        }
    }
}