using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Core.Interfaces.Crypto
{
    public interface IEd25519Crypto
    {
        /// <summary>
        /// Generates a new Ed25519 signing key pair.
        /// Public Key: 32 bytes.
        /// Private Key: 32 bytes (doesn't include public key like other libs).
        /// </summary>
        (byte[] PublicKey, byte[] PrivateKey) GenerateKeyPair();

        /// <summary>
        /// Signs a message using the private key.
        /// </summary>
        /// <param name="message">The data to sign (e.g., a serialized PreKey).</param>
        /// <param name="privateKey">The 64-byte private key.</param>
        /// <returns>64-byte signature.</returns>
        byte[] Sign(byte[] message, byte[] privateKey);

        /// <summary>
        /// Verifies a signature against a message and public key.
        /// </summary>
        /// <returns>True if valid, False if invalid.</returns>
        bool Verify(byte[] message, byte[] signature, byte[] publicKey);
    }
}