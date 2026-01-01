using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Core.Interfaces.Crypto
{
    public interface IAesCrypto
    {
        /// <summary>
        /// Encrypts input byte array
        /// </summary>
        /// <param name="plaintext"></param>
        /// <param name="key"></param>
        /// <returns>Byte array formatted as: [ Nonce (12 bytes) ] [ Encrypted Data (N bytes) ] [ Auth Tag (16 bytes) ]</returns>
        byte[] Encrypt(byte[] plaintext, byte[] key, byte[]? aad = null);

        /// <summary>
        /// Decrypts input byte array
        /// </summary>
        /// <param name="ciphertextWithNonceAndTag">Byte array formatted as: [ Nonce (12 bytes) ] [ Encrypted Data (N bytes) ] [ Auth Tag (16 bytes) ]</returns>
        /// <param name="key"></param>
        /// <returns>Plaintext byte array</returns>
        byte[] Decrypt(byte[] ciphertextWithNonceAndTag, byte[] key, byte[]? aad = null);
    }
}