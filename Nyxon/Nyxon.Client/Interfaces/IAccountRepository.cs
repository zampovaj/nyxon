using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Client.Interfaces
{
    public interface IAccountRepository
    {
        Task<AccountMetadataDto?> GetAccountDataAsync();
        Task<bool> ChangePasswordAsync(ChangePasswordRequest request);
        Task DeleteAccountAsync(DeleteAccountRequest request);
        Task<List<string>> GenerateInviteCodesAsync(int count = 1);
    }
}