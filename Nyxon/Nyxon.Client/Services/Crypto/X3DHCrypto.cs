using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Text;
using System.Security.Cryptography;

namespace Nyxon.Client.Services.Crypto
{
    public class X3DHCrypto : IX3DHCrypto
    {
        private static readonly byte[] Salt = new byte[32];
        private static readonly byte[] Info = Encoding.UTF8.GetBytes("Nyxon::X3DH::v1");

        private readonly ICryptoService _cryptoService;

        public X3DHCrypto(ICryptoService cryptoService)
        {
            _cryptoService = cryptoService;
        }

        // DH1 = DH(IK_A.priv, SPK_B.pub)
        // DH2 = DH(EK_A.priv, IK_B.pub)
        // DH3 = DH(EK_A.priv, SPK_B.pub)
        // DH4 = DH(EK_A.priv, OPK_B.pub)  // if OPK exists

        public X3DHResult CalculateInitiatorSecret(
            byte[] IK_A_priv,
            byte[] IK_B_pub,
            byte[] SPK_B_pub,
            byte[] OPK_B_pub)
        {
            var EphemeralKeyPair = _cryptoService.GenerateEphemeralKeyPair();
            if (EphemeralKeyPair == null)
                throw new ArgumentNullException($"X3DH failed: {nameof(EphemeralKeyPair)}");

            byte[] DH1 = null, DH2 = null, DH3 = null, DH4 = null;

            try
            {
                DH1 = _cryptoService.DeriveSharedSecret(IK_A_priv, SPK_B_pub);
                DH2 = _cryptoService.DeriveSharedSecret(EphemeralKeyPair.PrivateKey, IK_B_pub);
                DH3 = _cryptoService.DeriveSharedSecret(EphemeralKeyPair.PrivateKey, SPK_B_pub);
                DH4 = _cryptoService.DeriveSharedSecret(EphemeralKeyPair.PrivateKey, OPK_B_pub);

                var result = new X3DHResult(
                    publicEphemeralKey: EphemeralKeyPair.PublicKey,
                    sharedSecret: DeriveRootKey(DH1, DH2, DH3, DH4)
                );

                return result;
            }
            finally
            {
                if (DH1 != null) CryptographicOperations.ZeroMemory(DH1);
                if (DH2 != null) CryptographicOperations.ZeroMemory(DH2);
                if (DH3 != null) CryptographicOperations.ZeroMemory(DH3);
                if (DH4 != null) CryptographicOperations.ZeroMemory(DH4);
                if (EphemeralKeyPair.PrivateKey != null) CryptographicOperations.ZeroMemory(EphemeralKeyPair.PrivateKey);
            }
        }

        public X3DHResult CalculateInitiatorSecret(
            byte[] IK_A_priv,
            byte[] IK_B_pub,
            byte[] SPK_B_pub)
        {
            var EphemeralKeyPair = _cryptoService.GenerateEphemeralKeyPair();
            if (EphemeralKeyPair == null)
                throw new ArgumentNullException($"X3DH failed: {nameof(EphemeralKeyPair)}");

            byte[] DH1 = null, DH2 = null, DH3 = null;

            try
            {
                DH1 = _cryptoService.DeriveSharedSecret(IK_A_priv, SPK_B_pub);
                DH2 = _cryptoService.DeriveSharedSecret(EphemeralKeyPair.PrivateKey, IK_B_pub);
                DH3 = _cryptoService.DeriveSharedSecret(EphemeralKeyPair.PrivateKey, SPK_B_pub);

                var result = new X3DHResult(
                    publicEphemeralKey: EphemeralKeyPair.PublicKey,
                    sharedSecret: DeriveRootKey(DH1, DH2, DH3)
                );

                return result;
            }
            finally
            {
                if (DH1 != null) CryptographicOperations.ZeroMemory(DH1);
                if (DH2 != null) CryptographicOperations.ZeroMemory(DH2);
                if (DH3 != null) CryptographicOperations.ZeroMemory(DH3);
                if (EphemeralKeyPair.PrivateKey != null) CryptographicOperations.ZeroMemory(EphemeralKeyPair.PrivateKey);
            }
        }

        // DH1 = DH(SPK_B.priv, IK_A.pub)
        // DH2 = DH(IK_B.priv, EK_A.pub)
        // DH3 = DH(SPK_B.priv, EK_A.pub)
        // DH4 = DH(OPK_B.priv, EK_A.pub)  // if OPK was used

        public byte[] CalculateReceiverSecret(
            byte[] IK_B_priv,
            byte[] SPK_B_priv,
            byte[] OPK_B_priv,
            byte[] IK_A_pub,
            byte[] EK_A_pub)
        {
            byte[] DH1 = null, DH2 = null, DH3 = null, DH4 = null;

            try
            {
                DH1 = _cryptoService.DeriveSharedSecret(SPK_B_priv, IK_A_pub);
                DH2 = _cryptoService.DeriveSharedSecret(IK_B_priv, EK_A_pub);
                DH3 = _cryptoService.DeriveSharedSecret(SPK_B_priv, EK_A_pub);
                DH4 = _cryptoService.DeriveSharedSecret(OPK_B_priv, EK_A_pub);

                var result = DeriveRootKey(DH1, DH2, DH3, DH4);

                return result;
            }
            finally
            {
                if (DH1 != null) CryptographicOperations.ZeroMemory(DH1);
                if (DH2 != null) CryptographicOperations.ZeroMemory(DH2);
                if (DH3 != null) CryptographicOperations.ZeroMemory(DH3);
                if (DH4 != null) CryptographicOperations.ZeroMemory(DH4);
            }
        }

        public byte[] CalculateReceiverSecret(
            byte[] IK_B_priv,
            byte[] SPK_B_priv,
            byte[] IK_A_pub,
            byte[] EK_A_pub)
        {
            byte[] DH1 = null, DH2 = null, DH3 = null;

            try
            {
                DH1 = _cryptoService.DeriveSharedSecret(SPK_B_priv, IK_A_pub);
                DH2 = _cryptoService.DeriveSharedSecret(IK_B_priv, EK_A_pub);
                DH3 = _cryptoService.DeriveSharedSecret(SPK_B_priv, EK_A_pub);

                var result = DeriveRootKey(DH1, DH2, DH3);

                return result;
            }
            finally
            {
                if (DH1 != null) CryptographicOperations.ZeroMemory(DH1);
                if (DH2 != null) CryptographicOperations.ZeroMemory(DH2);
                if (DH3 != null) CryptographicOperations.ZeroMemory(DH3);
            }
        }

        private byte[] DeriveRootKey(byte[] DH1, byte[] DH2, byte[] DH3, byte[] DH4)
        {
            int length = DH1.Length + DH2.Length + DH3.Length + DH4.Length;
            byte[] buffer = new byte[length];
            int offset = 0;

            try
            {
                Buffer.BlockCopy(DH1, 0, buffer, offset, DH1.Length);
                offset += DH1.Length;
                Buffer.BlockCopy(DH2, 0, buffer, offset, DH2.Length);
                offset += DH2.Length;
                Buffer.BlockCopy(DH3, 0, buffer, offset, DH3.Length);
                offset += DH3.Length;
                Buffer.BlockCopy(DH4, 0, buffer, offset, DH4.Length);

                return KDF(buffer);
            }
            finally
            {
                if (buffer != null) CryptographicOperations.ZeroMemory(buffer);
            }
        }

        private byte[] DeriveRootKey(byte[] DH1, byte[] DH2, byte[] DH3)
        {
            int length = DH1.Length + DH2.Length + DH3.Length;
            byte[] buffer = new byte[length];

            int offset = 0;

            try
            {
                Buffer.BlockCopy(DH1, 0, buffer, offset, DH1.Length);
                offset += DH1.Length;
                Buffer.BlockCopy(DH2, 0, buffer, offset, DH2.Length);
                offset += DH2.Length;
                Buffer.BlockCopy(DH3, 0, buffer, offset, DH3.Length);

                return KDF(buffer);
            }
            finally
            {
                if (buffer != null) CryptographicOperations.ZeroMemory(buffer);
            }
        }

        private byte[] KDF(byte[] bytes)
        {
            return HKDF.DeriveKey(HashAlgorithmName.SHA256, bytes, 32, Salt, Info);
        }
    }
}