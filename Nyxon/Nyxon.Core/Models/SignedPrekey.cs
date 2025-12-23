using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Nyxon.Core.Models
{
    public class SignedPrekey
    {
        public Guid Id { get; set; }
        public byte[] PublicKey { get; set; }
        public byte[] PrivateKey { get; set; }
        public byte[] Signature { get; set; }

        public SignedPrekey(byte[] publicKey, byte[] privateKey, byte[] signature)
        {
            Id = Guid.NewGuid();
            PublicKey = publicKey;
            PrivateKey = privateKey;
            Signature = signature;
        }
    }
}