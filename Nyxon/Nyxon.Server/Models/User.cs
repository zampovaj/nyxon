using Nyxon.Core.Version;

namespace Nyxon.Server.Models
{
    public class User
    {
        public Guid Id { get; set; }

        public virtual UserVault UserVault { get; set; }

        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public byte[] Salt { get; set; } // 16 bytes
        public byte[] PublicKey { get; set; }
        public DateTime CreatedAt { get; set; }
        public short Version { get; set; }
        public bool Admin { get; set; }
        public bool CanCreateInvites { get; set; }

        public virtual ICollection<SignedPrekey> SignedPrekeys { get; set; } = new List<SignedPrekey>();
        public virtual ICollection<OneTimePrekey> OneTimePrekeys { get; set; } = new List<OneTimePrekey>();
        public virtual ICollection<ConversationVault> ConversationVaults { get; set; } = new List<ConversationVault>();
        public virtual ICollection<ConversationUser> ConversationUsers { get; set; } = new List<ConversationUser>();

        protected User() { }

        public User(Guid id, UserVault userVault, string username, string passwordHash, byte[] salt, byte[] publicKey, DateTime createdAt, short version, bool admin, bool canCreateInvites, SignedPrekey signedPrekeys, OneTimePrekey oneTimePrekeys, ICollection<ConversationVault> conversationVaults, ICollection<ConversationUser> conversationUsers = null)
        {
            Id = id;
            UserVault = userVault;
            Username = username;
            PasswordHash = passwordHash;
            Salt = salt;
            PublicKey = publicKey;
            CreatedAt = createdAt;
            Version = version;
            Admin = admin;
            CanCreateInvites = canCreateInvites;
            if (signedPrekeys != null)
            {
                SignedPrekeys.Add(signedPrekeys);
            }
            if (oneTimePrekeys != null)
            {
                OneTimePrekeys.Add(oneTimePrekeys);
            }
            if (conversationVaults != null)
            {
                ConversationVaults = conversationVaults;
            }
            if (conversationUsers != null)
            {
                ConversationUsers = conversationUsers;
            }
        }

        /// <summary>
        /// Creates a brand new user
        /// </summary>
        public User(string username, string passwordHash, byte[] salt, byte[] publicKey, bool admin, bool canCreateInvites)
        {
            Id = Guid.NewGuid();
            Username = username;
            PasswordHash = passwordHash;
            Salt = salt;
            PublicKey = publicKey;
            CreatedAt = DateTime.UtcNow;
            Version = AppVersion.Current;
            Admin = admin;
            CanCreateInvites = canCreateInvites;
        }
    }
}