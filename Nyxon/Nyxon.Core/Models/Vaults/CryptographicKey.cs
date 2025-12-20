using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Core.Models.Vaults
{
    public class CryptographicKey
    {
        public byte[] PublicKey { get; init; }
        public byte[] PrivateKey { get; init; }

        public CryptographicKey(byte[] publicKey, byte[] privateKey)
        {
            PublicKey = publicKey;
            PrivateKey = privateKey;
        }
    }
}