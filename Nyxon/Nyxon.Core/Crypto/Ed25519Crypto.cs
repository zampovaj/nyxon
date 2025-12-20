using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualBasic;

using Geralt;

namespace Nyxon.Core.Crypto
{
    public class Ed25519Crypto : IEd25519Crypto
    {
        public const int PublicKeySize = 32;
        public const int PrivateKeySize = 64;
        public const int SignatureSize = 64;

        public (byte[] PublicKey, byte[] PrivateKey) GenerateKeyPair()
        {
            var publicKey = new byte[PublicKeySize];
            var privateKey = new byte[PrivateKeySize];

            Ed25519.GenerateKeyPair(publicKey, privateKey);

            return (publicKey, privateKey);
        }

        public byte[] Sign(byte[] data, byte[] privateKey)
        {
            ValidateInput(data, nameof(data));
            ValidateKey(privateKey, PublicKeySize, nameof(privateKey));

            var signature = new byte[SignatureSize];

            Ed25519.Sign(signature, data, privateKey);

            return signature;
        }

        public bool Verify(byte[] data, byte[] signature, byte[] publicKey)
        {
            // verify doesnt need too descriptive errors, just return false
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (signature == null || signature.Length != SignatureSize) return false;
            if (publicKey == null || publicKey.Length != PublicKeySize) return false;

            return Ed25519.Verify(signature, data, publicKey);
        }

        // validation

        private static void ValidateInput(byte[] input, string paramName)
        {
            if (input == null)
                throw new ArgumentNullException(paramName);
            if (input.Length == 0)
                throw new ArgumentException("Input cannot be empty.", paramName);
        }

        private static void ValidateKey(byte[] key, int expectedSize, string paramName)
        {
            if (key == null)
                throw new ArgumentNullException(paramName);
            if (key.Length != expectedSize)
                throw new ArgumentException($"Key must be exactly {expectedSize} bytes.", paramName);
        }
    }
}