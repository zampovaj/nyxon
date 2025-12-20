using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Core.Interfaces.Crypto
{
    public interface IX25519Crypto
    {
        /// <summary>
        /// Generates a new Curve25519 key pair. 
        /// Use this for creating Identity Keys, PreKeys, and Ephemeral Keys.
        /// </summary>
        (byte[] PublicKey, byte[] PrivateKey) GenerateKeyPair();

        /// <summary>
        /// Computes the raw Diffie-Hellman shared secret between a local private key and a remote public key.
        /// output = curve25519(localPrivateKey, remotePublicKey)
        /// </summary>
        byte[] DeriveSharedSecret(byte[] localPrivateKey, byte[] remotePublicKey);
    }

}