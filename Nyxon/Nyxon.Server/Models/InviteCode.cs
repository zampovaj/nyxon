using Nyxon.Core.Version;

namespace Nyxon.Server.Models
{
    public class InviteCode
    {
        public Guid Id { get; set; }
        public byte[] CodeHash { get; set; }
        public bool Used { get; private set; }
        public short Version { get; set; }

        protected InviteCode() { }
        
        public InviteCode(Guid id, byte[] codeHash, bool used)
        {
            Id = id;
            CodeHash = codeHash;
            Used = used;
            Version = AppVersion.Current;
        }

        /// <summary>
        /// Creates a brand new invite code 
        /// </summary>
        public InviteCode(byte[] codeHash)
        {
            Id = Guid.NewGuid();
            CodeHash = codeHash;
            Used = false;
            Version = AppVersion.Current;
        }

        public void Use()
        {
            Used = true;
        }
    }
}