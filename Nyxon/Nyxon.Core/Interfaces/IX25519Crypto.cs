using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Core.Interfaces.Crypto
{
    public interface IX25519Crypto
    {
        (byte[] PublicKey, byte[] PrivateKey) GenerateKeyPair();
        byte[] DeriveSharedSecret(byte[] localPrivateKey, byte[] remotePublicKey);
    }

}