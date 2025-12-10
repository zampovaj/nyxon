using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Models
{
    public class UserContext
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;

        public string Token { get; set; } = string.Empty;
        public byte[] PassphraseKey { get; set; } = Array.Empty<byte>();

        // validate model
        public bool IsValid => !string.IsNullOrEmpty(Token) && MasterKey.Length > 0;
    }
}