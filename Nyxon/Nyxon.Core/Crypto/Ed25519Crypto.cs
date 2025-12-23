using System;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Security;

namespace Nyxon.Core.Crypto
{
    public class Ed25519Crypto : IEd25519Crypto
    {
        public const int PublicKeySize = 32;
        public const int SignatureSize = 64;
        private readonly SecureRandom _secureRandom = new SecureRandom();

        public (byte[] PublicKey, byte[] PrivateKey) GenerateKeyPair()
        {
            var gen = new Ed25519KeyPairGenerator();
            gen.Init(new Ed25519KeyGenerationParameters(_secureRandom));
            
            var pair = gen.GenerateKeyPair();
            
            // bouncycastle generates 32 byte private keys, unlike other libs,
            // which use 64 byte keys (private + public),
            // because both public an dprivaet keys atre needed for signing
            // bouncycastle can derive public key from private key automatically
            return (
                ((Ed25519PublicKeyParameters)pair.Public).GetEncoded(), 
                ((Ed25519PrivateKeyParameters)pair.Private).GetEncoded()
            );
        }

        public byte[] Sign(byte[] data, byte[] privateKey)
        {
            if (data == null || data.Length == 0)
                throw new ArgumentException("Data required", nameof(data));
            if (privateKey == null)
                throw new ArgumentNullException(nameof(privateKey));

            var signer = new Ed25519Signer();
            
            // bouncy castle only needs 32 byte private key for signing
            Ed25519PrivateKeyParameters privParam;
            if (privateKey.Length == 32)
                privParam = new Ed25519PrivateKeyParameters(privateKey, 0);
            else
                throw new ArgumentException("Private key must be 32 or 64 bytes");

            signer.Init(true, privParam);
            signer.BlockUpdate(data, 0, data.Length);
            
            return signer.GenerateSignature();
        }

        public bool Verify(byte[] data, byte[] signature, byte[] publicKey)
        {
            if (data == null || signature == null || publicKey == null) return false;
            if (signature.Length != SignatureSize || publicKey.Length != PublicKeySize) return false;

            try
            {
                var signer = new Ed25519Signer();
                signer.Init(false, new Ed25519PublicKeyParameters(publicKey, 0));
                signer.BlockUpdate(data, 0, data.Length);
                return signer.VerifySignature(signature);
            }
            catch
            {
                return false;
            }
        }
    }
}