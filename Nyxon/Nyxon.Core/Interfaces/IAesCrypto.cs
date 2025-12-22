using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Core.Interfaces.Crypto
{
    public interface IAesCrypto
    {
        byte[] Encrypt(byte[] plaintext, byte[] key);
        byte[] Decrypt(byte[] ciphertextWithNonceAndTag, byte[] key);
    }
}