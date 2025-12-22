using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Core.Models
{
    public class OneTimePrekey
    {
        public Guid Id { get; set; }
        public byte[] PublicKey { get; set; }
        public byte[] PrivateKey { get; set; }

        public OneTimePrekey(byte[] publicKey, byte[] privateKey)
        {
            Id = Guid.NewGuid();
            PublicKey = publicKey;
            PrivateKey = privateKey;
        }
    }
}