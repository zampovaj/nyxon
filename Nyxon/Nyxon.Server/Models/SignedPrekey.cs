using Nyxon.Core.Version;

// signed_prekeys
// -------------
// id           : uuid pk
// user_id      : references users(id)
// public_key   : bytea
// encrypted_key: bytea
// signature    : bytea
// created_at   : datetime
// version      : smallint


namespace Nyxon.Server.Models
{
    public class SignedPrekey
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public virtual User User { get; set; }
        public byte[] PublicKey { get; set; }
        public byte[] EncryptedKey { get; set; }
        public byte[] Signature { get; set; }
        public DateTime CreatedAt { get; set; }
        public short Version { get; set; }

        protected SignedPrekey() { }

        public SignedPrekey(Guid id, Guid userId, byte[] publicKey, byte[] encryptedKey, byte[] signature, DateTime createdAt, short version)
        {
            Id = id;
            UserId = userId;
            PublicKey = publicKey;
            EncryptedKey = encryptedKey;
            Signature = signature;
            CreatedAt = createdAt;
            Version = version;
        }

        /// <summary>
        /// Creates a new signed prekey
        /// </summary>
        public SignedPrekey(Guid userId, byte[] publicKey, byte[] encryptedKey, byte[] signature)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            PublicKey = publicKey;
            EncryptedKey = encryptedKey;
            Signature = signature;
            CreatedAt = DateTime.UtcNow;
            Version = AppVersion.Current;
        }
    }
}