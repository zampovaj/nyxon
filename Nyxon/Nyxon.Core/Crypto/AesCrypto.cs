using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSec.Cryptography;

namespace Nyxon.Core.Crypto
{
    public class AesCrypto : IAesCrypto
    {
        private const int NonceSize = 12;
        private const int TagSize = 16;

        private readonly IRandomService _randomService;

        public AesCrypto(IRandomService randomService)
        {
            _randomService = randomService;
        }

        public byte[] Decrypt(byte[] ciphertextWithNonceAndTag, byte[] key)
        {
            using var aes = new AesGcm(key, TagSize);

            var nonce = new byte[NonceSize];
            byte[] ciphertext = new byte[ciphertextWithNonceAndTag.Length - NonceSize - TagSize];
            byte[] tag = new byte[TagSize];

            Buffer.BlockCopy(ciphertextWithNonceAndTag, 0, nonce, 0, NonceSize);
            Buffer.BlockCopy(ciphertextWithNonceAndTag, NonceSize, tag, 0, TagSize);
            Buffer.BlockCopy(ciphertextWithNonceAndTag, NonceSize + TagSize, ciphertext, 0, ciphertext.Length);

            byte[] plaintext = new byte[ciphertext.Length];

            aes.Decrypt(nonce, ciphertext, tag, plaintext);

            return plaintext;
        }

        public byte[] Encrypt(byte[] plaintext, byte[] key)
        {
            using var aes = new AesGcm(key, TagSize);

            var nonce = _randomService.GenerateRandomBytes(NonceSize);
            byte[] ciphertext = new byte[plaintext.Length];
            byte[] tag = new byte[TagSize];

            aes.Encrypt(nonce, plaintext, ciphertext, tag);

            byte[] result = new byte[NonceSize + TagSize + ciphertext.Length];
            Buffer.BlockCopy(nonce, 0, result, 0, NonceSize);
            Buffer.BlockCopy(tag, 0, result, NonceSize, TagSize);
            Buffer.BlockCopy(ciphertext, 0, result, NonceSize + TagSize, ciphertext.Length);

            return result;
        }
    }
}