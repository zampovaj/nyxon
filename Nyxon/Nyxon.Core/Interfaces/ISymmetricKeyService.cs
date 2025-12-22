using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Core.Interfaces
{
    public interface ISymmetricKeyService
    {
        byte[] GenerateVaultKey();
        byte[] DeriveKeyFromPassphrase(byte[] passphrase, byte[] salt, int length = 32);
    }
}