using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nyxon.Client.Interfaces.Crypto;

namespace Nyxon.Client.Services.Crypto
{
    public class CryptoService : ICryptoService
    {
        private readonly IKeyGenerationService _keyGenerationService;
        private readonly IArgon2Crypto _argon2Crypto;

        public CryptoService(IKeyGenerationService keyGenerationService, IArgon2Crypto argon2Crypto)
        {
            _keyGenerationService = keyGenerationService;
            _argon2Crypto = argon2Crypto;
        }

        public async Task<byte[]> DerivePassphraseKeyAsync(byte[] passphrase, byte[] salt)
        {
            return await _argon2Crypto.DerivePassphraseKeyAsync(passphrase, salt);
        }

        public byte[] EncryptWithKey(byte[] data, byte[] key)
        {
            return _keyGenerationService.EncryptWithKey(data, key);
        }

        public byte[] DecryptWithKey(byte[] data, byte[] key)
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