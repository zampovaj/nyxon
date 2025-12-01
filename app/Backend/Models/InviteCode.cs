using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shared.Version;

/*id : uuid pk ai
code_hash : string
used : bool*/

namespace Backend.Models
{
    public class InviteCode
    {
        public Guid Id { get; set; }
        public string CodeHash { get; set; }
        public bool Used { get; private set; }
        public short Version { get; set; }

        public InviteCode(Guid id, string codeHash, bool used)
        {
            Id = id;
            CodeHash = codeHash;
            Used = used;
            Version = AppVersion.Current;
        }

        /// <summary>
        /// Creates a brand new invite code 
        /// </summary>
        public InviteCode(string codeHash)
        {
            Id = Guid.NewGuid();
            CodeHash = codeHash;
            Used = false;
            Version = AppVersion.Current;
        }
    }
}