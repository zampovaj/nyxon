using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System.Security.Cryptography;

namespace Nyxon.Core.Crypto
{
    public class AesCrypto : IAesCrypto
    {
        private const int NonceSize = 12;
        private const int TagSize = 16;
        private const int KeySize = 32;

        private readonly SecureRandom _secureRandom = new SecureRandom();

        public byte[] Encrypt(byte[] plaintext, byte[] key)
        {
            ValidateInput(plaintext, nameof(plaintext));
            ValidateKey(key, nameof(key));

            var nonce = new byte[NonceSize];
            _secureRandom.NextBytes(nonce);

            var cipher = new GcmBlockCipher(new AesEngine());
            var parameters = new AeadParameters(new KeyParameter(key), TagSize * 8, nonce);
            cipher.Init(true, parameters); // true = encryption

            //prepare output
            // getciphertext() returns the prediction of how many bytes will get used based on input and tag
            var outpuSize = cipher.GetOutputSize(plaintext.Length);
            var result = new byte[NonceSize + outpuSize];

            Array.Copy(nonce, 0, result, 0, NonceSize);

            // procesbytes() writes result into the result byte array and returns the number of bytes written
            int length = cipher.ProcessBytes(plaintext, 0, plaintext.Length, result, NonceSize);
            //append tag at the end of result array
            cipher.DoFinal(result, NonceSize + length);

            return result;
        }

        public byte[] Decrypt(byte[] ciphertextWithNonceAndTag, byte[] key)
        {
            if (ciphertextWithNonceAndTag == null || ciphertextWithNonceAndTag.Length < NonceSize + TagSize)
                throw new ArgumentException("Ciphertext is too short.", nameof(ciphertextWithNonceAndTag));

            ValidateKey(key, nameof(key));

            var nonce = new byte[NonceSize];
            Array.Copy(ciphertextWithNonceAndTag, 0, nonce, 0, NonceSize);

            var cipher = new GcmBlockCipher(new AesEngine());
            var parameters = new AeadParameters(new KeyParameter(key), TagSize * 8, nonce);
            cipher.Init(false, parameters); // false = decryption

            // input for decryption is everyhting after nonce
            var cipherTextLength = ciphertextWithNonceAndTag.Length - NonceSize;
            //prepare output
            var plaintext = new byte[cipher.GetOutputSize(cipherTextLength)];

            // bouncy castle throws an exception automatically if tag doesnt match
            // convert the exception to .net and pass further
            try
            {
                // getciphertext() returns the prediction of how many bytes will get used based on input and tag
                int length = cipher.ProcessBytes(ciphertextWithNonceAndTag, NonceSize, cipherTextLength, plaintext, 0);
                cipher.DoFinal(plaintext, length);
            }
            catch (InvalidCipherTextException ex)
            {
                throw new System.Security.Cryptography.CryptographicException("Decryption failed (Auth Tag mismatch).", ex);
            }

            return plaintext;

        }

        //validation
        private static void ValidateKey(byte[] key, string paramName)
        {
            if (key == null) throw new ArgumentNullException(paramName);
            if (key.Length != KeySize) throw new ArgumentException($"Key must be {KeySize} bytes.", paramName);
        }

        private static void ValidateInput(byte[] input, string paramName)
        {
            if (input == null) throw new ArgumentNullException(paramName);
        }
    }
}