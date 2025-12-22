using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Client.Services.Crypto
{
    public class CryptoService : ICryptoService
    {
        private readonly IKeyGenerationService _keyGenerationService;

        public CryptoService(IKeyGenerationService keyGenerationService)
        {
            _keyGenerationService = keyGenerationService;
        }

        public byte[] DerivePassphraseKey(string passphrase, byte[] salt)
        {
            return _keyGenerationService.DeriveKeyFromPassphrase(passphrase, salt);
        }

        public byte[] EncryptKey(byte[] data, byte[] key)
        {
            return _keyGenerationService.EncryptWithKey(data, key);
        }

        public byte[] DecryptKey(byte[] data, byte[] key)
        {
            return _keyGenerationService.DecryptWithKey(data, key);
        }

        public AsymmetricKey GenerateIdentityKey()
        {
            return _keyGenerationService.GenerateIdentityKeyPair();
        }

        public byte[] GeneratePassphraseSalt()
        {
            return _keyGenerationService.GenerateRandomSalt(32);
        }

        public byte[] GeneratePasswordSalt()
        {
            return _keyGenerationService.GenerateRandomSalt(16);
        }

        public byte[] GenerateVaultKey()
        {
            return _keyGenerationService.GenerateVaultKey();
        }
        
        public PrekeyBundle GeneratePrekeyBundle(byte[] privateIdentityKey, byte[] vaultKey, int opkCount)
        {
            List<OneTimePrekey> opkList = new List<OneTimePrekey>();

            var spk = _keyGenerationService.GenerateSPK(privateIdentityKey);
            spk.PrivateKey = _keyGenerationService.EncryptWithKey(spk.PrivateKey, vaultKey);
            
            for (int i = 0; i < opkCount; i++)
            {
                var opk = _keyGenerationService.GenerateOPK();
                opk.PrivateKey = _keyGenerationService.EncryptWithKey(opk.PrivateKey, vaultKey);

                if (opk == null) throw new ArgumentNullException(nameof(opk), "OneTimePrekey generation failed.");
                
                opkList.Add(opk);
            }

            return new PrekeyBundle(spk, opkList);
        }
    }
}