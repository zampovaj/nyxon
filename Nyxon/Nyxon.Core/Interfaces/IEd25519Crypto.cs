using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Core.Interfaces.Crypto
{
    public interface IEd25519Crypto
    {
        (byte[] PublicKey, byte[] PrivateKey) GenerateKeyPair();
        byte[] Sign(byte[] data, byte[] privateKey);
        bool Verify(byte[] data, byte[] signature, byte[] publicKey);
    }

}