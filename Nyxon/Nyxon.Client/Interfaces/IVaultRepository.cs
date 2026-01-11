using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Client.Interfaces
{
    public interface IVaultRepository
    {
        Task<UserVaultResponse?> FetchUserVaultAsync();
        Task<bool> CheckSignedPrekeyAsync();
        Task SaveNewSpkAsync(SignedPrekey spk);
    }
}