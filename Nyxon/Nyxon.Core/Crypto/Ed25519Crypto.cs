using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using NSec.Cryptography;

namespace Nyxon.Core.Crypto
{
    public class Ed25519Crypto : IEd25519Crypto
    {
        public (byte[] PublicKey, byte[] PrivateKey) GenerateKeyPair()
        {
            var key = new Key(SignatureAlgorithm.Ed25519);
            return (key.PublicKey.Export(KeyBlobFormat.RawPublicKey),
                key.Export(KeyBlobFormat.RawPrivateKey));
        }

        public byte[] Sign(byte[] data, byte[] privateKey)
        {
            var key = Key.Import(SignatureAlgorithm.Ed25519, privateKey, KeyBlobFormat.RawPrivateKey);
            return SignatureAlgorithm.Ed25519.Sign(key, data);
        }

        public bool Verify(byte[] data, byte[] signature, byte[] publicKey)
        {
            var key = PublicKey.Import(SignatureAlgorithm.Ed25519, publicKey, KeyBlobFormat.RawPublicKey);
            return SignatureAlgorithm.Ed25519.Verify(key, data, signature);
        }
    }
}