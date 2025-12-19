using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Core.Interfaces.Crypto
{
    public interface ISymmetricCrypto
    {
        byte[] Encrypt(byte[] plaintext, byte[] key, byte[] nonce);
        byte[] Decrypt(byte[] ciphertext, byte[] key, byte[] nonce);
        byte[] HKDF(byte[] inputKeyMaterial, byte[] salt, byte[] info, int length);
    }

}