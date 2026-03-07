using Microsoft.JSInterop;
using System.Security.Cryptography;
using Nyxon.Client.Interfaces.Crypto;

namespace Nyxon.Client.Crypto
{

    public sealed class Argon2CryptoWasm : IArgon2Crypto
    {
        private readonly IJSRuntime _js;

        public Argon2CryptoWasm(IJSRuntime js)
        {
            _js = js;
        }

        public async Task<byte[]> DeriveKeyAsync(
            byte[] passphrase,
            byte[] salt,
            int length,
            int iterations,
            int memoryKb,
            int parallelism)
        {
            // base64 cause js is pain
            var passB64 = Convert.ToBase64String(passphrase);
            var saltB64 = Convert.ToBase64String(salt);

            try
            {
                var resultB64 = await _js.InvokeAsync<string>(
                    "nyxonArgon2.deriveKey",
                    passB64,
                    saltB64,
                    length,
                    iterations,
                    memoryKb,
                    parallelism
                );

                return Convert.FromBase64String(resultB64);
            }
            finally
            {
                // wipe buffers
                CryptographicOperations.ZeroMemory(passphrase);
            }
        }
        public async Task<byte[]> DerivePassphraseKeyAsync(byte[] passphrase, byte[] salt)
        {
            return await DeriveKeyAsync(
                passphrase,
                salt,
                length: 32,
                iterations: 2,
                memoryKb: 64 * 1024,
                parallelism: 1
            );
        }
        public async Task<byte[]> HashPasswordAsync(byte[] password, byte[] salt)
        {
            return await DeriveKeyAsync(
                password,
                salt,
                length: 32,
                iterations: 1,
                memoryKb: 64 * 1024,
                parallelism: 1
            );
        }

    }
}