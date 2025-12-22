using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Core.Models
{
    public class AsymmetricKey
    {
        public byte[] PublicKey { get; init; }
        public byte[] PrivateKey { get; init; }

        public AsymmetricKey(byte[] publicKey, byte[] privateKey)
        {
            PublicKey = publicKey;
            PrivateKey = privateKey;
        }
    }
}