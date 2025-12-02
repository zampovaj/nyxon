/*user_id : uuid
version : small int
updated_at : datetime
vault_key : bytes / encrypted with passphrase key
identity_key : bytes / encrypted with vault key*/

namespace Backend.Models
{
    public class UserVault
    {
        public Guid UserId { get; set; }
        public virtual User User { get; set; }

        public DateTime UpdatedAt { get; set; }
        public short Version { get; set; }
        public byte[] VaultKey { get; set; }
        public byte[] IdentityKey { get; set; }

        protected UserVault() {}

        public UserVault(Guid userId, DateTime updatedAt, short version, byte[] vaultKey, byte[] identityKey)
        {
            UserId = userId;
            UpdatedAt = updatedAt;
            Version = version;
            VaultKey = vaultKey;
            IdentityKey = identityKey;
        }

        /// <summary>
        /// Creates a brand new user vault
        /// </summary>
        public UserVault(Guid userId, byte[] vaultKey, byte[] identityKey)
        {
            UserId = userId;
            UpdatedAt = DateTime.UtcNow;
            Version = AppVersion.Current;
            VaultKey = vaultKey;
            IdentityKey = identityKey;
        }
    }
}