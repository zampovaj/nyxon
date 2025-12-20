using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Geralt;

namespace Nyxon.Core.Crypto
{
    public class X25519Crypto : IX25519Crypto
    {
        private const int KeySize = 32; // standard for curve25519
        public (byte[] PublicKey, byte[] PrivateKey) GenerateKeyPair()
        {
            var publicKey = new byte[KeySize];
            var privateKey = new byte[KeySize];

            X25519.GenerateKeyPair(publicKey, privateKey);

            return (publicKey, privateKey);
        }

        public byte[] DeriveSharedSecret(byte[] localPrivateKey, byte[] remotePublicKey)
        {
            ValidateKey(remotePublicKey, nameof(remotePublicKey));
            ValidateKey(localPrivateKey, nameof(localPrivateKey));

            var sharedSecret = new byte[KeySize];

            try
            {
                X25519.ComputeSharedSecret(sharedSecret, localPrivateKey, remotePublicKey);
            }
            catch (Exception ex) when (ex is CryptographicException || ex is ArgumentException)
            {
                throw new InvalidOperationException("Failed to compute X25519 shared secret. The remote key may be invalid.", ex);
            }

            return sharedSecret;
        }

        // validation

        private static void ValidateKey(byte[] key, string paramName)
        {
            if (key == null)
            {
                throw new ArgumentNullException(paramName);
            }

            if (key.Length != KeySize)
            {
                throw new ArgumentException($"X25519 key must be exactly {KeySize} bytes, but was {key.Length}.", paramName);
            }
        }

    }
}