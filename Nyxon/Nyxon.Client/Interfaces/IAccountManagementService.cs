using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Client.Interfaces
{
    public interface IAccountManagementService
    {
        Task<AccountMetadataDto> FetchAccountDataAsync();
        Task ChangePasswordAsync(byte[] currentPasswordBytes, byte[] newPasswordBytes);
        Task DeleteAccountAsync(byte[] passwordBytes);
    }
}