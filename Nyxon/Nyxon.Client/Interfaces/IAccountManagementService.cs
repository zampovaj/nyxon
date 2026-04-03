using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Client.Interfaces
{
    public interface IAccountManagementService
    {
        Task<AccountMetadataDto> FetchAccountDataAsync();
        Task<bool> ChangePasswordAsync(byte[] currentPasswordBytes, byte[] oldPasswordBytes);
        Task DeleteAccountAsync(byte[] passwordBytes);
    }
}