/*user_id : uuid
version : small int
updated_at : datetime
vault_key : bytes / encrypted with passphrase key
identity_key : bytes / encrypted with vault key*/


/*user_id : uuid
version : small int
updated_at : datetime
vault_key : bytes / encrypted with passphrase key
identity_key : bytes / encrypted with vault key*/

using Nyxon.Core.Version;

namespace Nyxon.Server.Models
{
    public class UserVault
    {
        public Guid UserId { get; set; }
        public virtual User User { get; set; }

        public DateTime UpdatedAt { get; set; }
        public short Version { get; set; }
        public byte[] PassphraseSalt { get; set; } // 32 bytes
        public byte[] VaultKey { get; set; }
        public byte[] PrivateIdentityKey { get; set; }
        public byte[] PrivateAgreementKey { get; set; }

        protected UserVault() {}

        public UserVault(Guid userId, DateTime updatedAt, short version, byte[] passphraseSalt, byte[] vaultKey, byte[] privateIdentityKey, byte[] privateAgreementKey)
        {
            UserId = userId;
            UpdatedAt = updatedAt;
            Version = version;
            PassphraseSalt = passphraseSalt;
            VaultKey = vaultKey;
            PrivateIdentityKey = privateIdentityKey;
            PrivateAgreementKey = privateAgreementKey;
        }

        /// <summary>
        /// Creates a brand new user vault
        /// </summary>
        public UserVault(Guid userId, byte[] passphraseSalt, byte[] vaultKey, byte[] privateIdentityKey, byte[] privateAgreementKey)
        {
            UserId = userId;
            UpdatedAt = DateTime.UtcNow;
            Version = AppVersion.Current;
            PassphraseSalt = passphraseSalt;
            VaultKey = vaultKey;
            PrivateIdentityKey = privateIdentityKey;
            PrivateAgreementKey = privateAgreementKey;
        }
    }
}